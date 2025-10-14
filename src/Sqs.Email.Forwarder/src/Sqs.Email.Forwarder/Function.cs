using System.Net;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using MimeKit;
using System.Text.Json;
using Sqs.Email.Forwarder.Models;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Sqs.Email.Forwarder;

public class Function
{
    private readonly string[] _mailBuckets;

    private readonly string _awsRegion;

    private readonly IAmazonS3 _s3Client;

    private readonly IAmazonSimpleEmailService _sesClient;

    private readonly string[] _emailSenders;

    private readonly string _emailRecipient;

    // ReSharper disable once ConvertConstructorToMemberInitializers
    public Function()
    {
        _mailBuckets = (Environment.GetEnvironmentVariable("MAIL_S3_BUCKETS") ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if(!_mailBuckets.Any())
            throw new InvalidOperationException("No S3 buckets configured in MAIL_S3_BUCKETS environment variable");

        _emailSenders = (Environment.GetEnvironmentVariable("MAIL_SENDERS") ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if(!_emailSenders.Any())
            throw new InvalidOperationException("No email senders configured in MAIL_SENDERS environment variable");

        _emailRecipient = Environment.GetEnvironmentVariable("MAIL_RECIPIENT") ?? throw new InvalidOperationException("MAIL_RECIPIENT environment variable not set");

        _awsRegion = Environment.GetEnvironmentVariable("AWS_REGION")!;

        _s3Client = new AmazonS3Client(Amazon.RegionEndpoint.GetBySystemName(_awsRegion));
        _sesClient = new AmazonSimpleEmailServiceClient(Amazon.RegionEndpoint.GetBySystemName(_awsRegion));
    }

    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used
    /// to respond to SQS messages.
    /// </summary>
    /// <param name="evnt">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        // do not parallelize. Messages are read into memory. We don't want to read too many at once.
        foreach (var message in evnt.Records)
            await ProcessMessageAsync(message, context);
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        var messageId = ExtractSesMessageId(message.Body);

        context.Logger.LogInformation($"Processing SES messageId: {messageId}");

        var emailInfo = await GetMessageFromS3Async(messageId, context);
        var forwardedEmail = await CreateForwardedEmailAsync(emailInfo);
        var sendResult = await SendEmailAsync(emailInfo, forwardedEmail);

        context.Logger.LogInformation(sendResult);
    }

    private static string ExtractSesMessageId(string sqsBody)
    {
        using var doc = JsonDocument.Parse(sqsBody);
        var messageId = doc.RootElement
            .GetProperty("Message")
            .GetString() ?? throw new InvalidOperationException("SQS message is not in expected format.");

        using var innerDoc = JsonDocument.Parse(messageId);
        return innerDoc.RootElement
            .GetProperty("mail")
            .GetProperty("messageId")
            .GetString() ?? throw new InvalidOperationException("Message is not in expected format.");
    }

    private async Task<EmailInfo> GetMessageFromS3Async(string messageId, ILambdaContext context)
    {
        var bucketIndex = 0;
        foreach(var bucket in _mailBuckets)
        {
            try
            {
                await _s3Client.GetObjectMetadataAsync(new GetObjectMetadataRequest
                {
                    BucketName = bucket,
                    Key = messageId
                });
            } catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                context.Logger.LogInformation($"Email with messageId {messageId} not found in bucket {bucket}, exception: {ex.Message}");
                bucketIndex++;
                continue;
            }

            var obj = await _s3Client.GetObjectAsync(new GetObjectRequest
            {
                BucketName = bucket,
                Key = messageId
            });

            await using var ms = new MemoryStream();
            await obj.ResponseStream.CopyToAsync(ms);
            var rawEmail = ms.ToArray();
            return new EmailInfo
            {
                MessageId = messageId,
                Resender = _emailSenders[bucketIndex],
                RawEmail = rawEmail,
                Url = $"https://s3.console.aws.amazon.com/s3/object/{bucket}/{messageId}?region={_awsRegion}"
            };
        }

        throw new InvalidRequestException("Email not found in any configured bucket");
    }

    private async Task<string> CreateForwardedEmailAsync(EmailInfo emailInfo)
    {
        await using var messageStream = new MemoryStream(emailInfo.RawEmail);

        using var mailObject = await MimeMessage.LoadAsync(messageStream);
        var subjectOriginal = mailObject.Subject ?? "(no subject)";

        var subject = subjectOriginal.StartsWith("FW:") || subjectOriginal.StartsWith("FWD:")
            ? subjectOriginal
            : $"FW: {subjectOriginal}";

        // Extract readable body
        var extractedBody = ExtractBody(mailObject);

        var bodyHtml = $"""
        <html><body>
        <p><strong>Forwarded message:</strong></p>
        <p><strong>From:</strong> {mailObject.From}<br>
           <strong>To:</strong> {mailObject.To}<br>
           <strong>Date:</strong> {mailObject.Date}<br>
           <strong>Subject:</strong> {subjectOriginal}</p>
        <hr>
        <pre style='font-family: sans-serif; white-space: pre-wrap;'>{extractedBody}</pre>
        <hr>
        <p>Original message archived at <a href='{emailInfo.Url}'>{emailInfo.Url}</a></p>
        </body></html>
        """;

        using var msg = new MimeMessage();
        msg.Subject = subject;
        msg.From.Add(MailboxAddress.Parse(emailInfo.Resender));
        msg.To.Add(MailboxAddress.Parse(_emailRecipient));

        var builder = new BodyBuilder { HtmlBody = bodyHtml };

        // Attach original .eml
        var filename = new string(subjectOriginal.Where(char.IsLetterOrDigit).ToArray());
        if (filename.Length > 50) filename = filename[..50];

        builder.Attachments.Add($"{filename}.eml", emailInfo.RawEmail, new ContentType("message", "rfc822"));
        msg.Body = builder.ToMessageBody();

        return msg.ToString();
    }

    private static string ExtractBody(MimeMessage mailObject)
    {
        switch (mailObject.Body)
        {
            // Try to get plain text, fallback to HTML
            case Multipart multipart:
            {
                foreach (var part in multipart)
                    if (part is TextPart { IsPlain: true } textPart)
                        return textPart.Text;

                foreach (var part in multipart)
                    if (part is TextPart { IsHtml: true } textPart)
                        return textPart.Text;

                break;
            }
            case TextPart text:
                return text.Text;
        }

        return "(No readable message body found)";
    }

    private async Task<string> SendEmailAsync(EmailInfo emailInfo, string forwardedEmail)
    {
        await using var rawMessageStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(forwardedEmail));

        var req = new SendRawEmailRequest
        {
            Source = emailInfo.Resender,
            Destinations = [ _emailRecipient ],
            RawMessage = new RawMessage(rawMessageStream),
        };
        await _sesClient.SendRawEmailAsync(req);
        return $"Email sent! Message ID: {emailInfo.MessageId}";
    }
}