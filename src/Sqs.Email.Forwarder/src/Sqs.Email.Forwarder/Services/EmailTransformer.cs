using MimeKit;
using Sqs.Email.Forwarder.Models;

namespace Sqs.Email.Forwarder.Services;

internal class EmailTransformer
{
    private readonly ExtractionService  _extractionService;

    private readonly Config _config;

    public EmailTransformer(
        Config config,
        ExtractionService extractionService
        )
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _extractionService = extractionService ?? throw new ArgumentNullException(nameof(extractionService));
    }

    public async Task<string> TransformToForwardedEmailAsync(EmailInfo emailInfo)
    {
        await using var messageStream = new MemoryStream(emailInfo.RawEmail);

        using var mailObject = await MimeMessage.LoadAsync(messageStream);
        var subjectOriginal = mailObject.Subject ?? "(no subject)";

        var sender = _extractionService.ExtractSenderInfo(mailObject.From);
        var recipient = _extractionService.ExtractRelevantRecipientInfo(mailObject.To, emailInfo.Domain);

        var subject = $"[{sender.FriendlyName}]➡️[{recipient.LocalPart}] {subjectOriginal}";

        // Extract readable body
        var extractedBody = _extractionService.ExtractBody(mailObject);

        var bodyHtml = $"""
                        <html><body>
                        <p><strong>Forwarded message:</strong></p>
                        <p><strong>From:</strong> {sender.FriendlyName} | {sender.EmailAddress}<br />
                           <strong>To:</strong> {mailObject.To}<br />
                           <strong>Date:</strong> {mailObject.Date}<br />
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
        msg.To.Add(MailboxAddress.Parse(_config.EmailRecipient));

        var builder = new BodyBuilder { HtmlBody = bodyHtml };

        // Attach original .eml
        var filename = new string(subjectOriginal.Where(char.IsLetterOrDigit).ToArray());
        if (filename.Length > 50) filename = filename[..50];

        builder.Attachments.Add($"{filename}.eml", emailInfo.RawEmail, new ContentType("message", "rfc822"));
        msg.Body = builder.ToMessageBody();

        return msg.ToString();
    }
}