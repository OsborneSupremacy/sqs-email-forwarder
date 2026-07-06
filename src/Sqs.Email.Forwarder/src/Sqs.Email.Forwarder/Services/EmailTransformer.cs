using MimeKit;

namespace Sqs.Email.Forwarder.Services;

internal class EmailTransformer : IEmailTransformer
{
    private readonly ILogger<EmailTransformer> _logger;

    private readonly IExtractionService  _extractionService;

    private readonly Config _config;

    public EmailTransformer(
        ILogger<EmailTransformer> logger,
        Config config,
        IExtractionService extractionService
        )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _extractionService = extractionService ?? throw new ArgumentNullException(nameof(extractionService));
    }

    public async Task<string> TransformToForwardedEmailAsync(EmailInfo emailInfo)
    {
        using var mailObject = await GetMailObjectAsync(emailInfo)
            .ConfigureAwait(false);

        var generated = ComposeForwardedEmail(emailInfo, mailObject);

        return TransformToMimeString(generated);
    }

    private async Task<MimeMessage> GetMailObjectAsync(EmailInfo emailInfo)
    {
        await using var messageStream = new MemoryStream(emailInfo.RawEmail);
        return await MimeMessage
            .LoadAsync(messageStream)
            .ConfigureAwait(false);
    }

    private ForwardedEmailInfo ComposeForwardedEmail(EmailInfo emailInfo, MimeMessage mailObject)
    {
        var subjectOriginal = mailObject.Subject ?? "(no subject)";

        var sender = _extractionService.ExtractSenderInfo(mailObject.From);
        var recipient = _extractionService.ExtractRelevantRecipientInfo(mailObject.To, emailInfo.Domain);

        var subject = $"[{sender.FriendlyName}]➡️[{recipient.LocalPart}] {subjectOriginal}";

        var extractedBody = _extractionService.ExtractBody(mailObject);

        var bodyHtml = $"""
                        <html><body>
                        <p><strong>Forwarded message:</strong></p>
                        <p><strong>From:</strong> {sender.FriendlyName} | {sender.EmailAddress}<br />
                           <strong>To:</strong> {mailObject.To}<br />
                           <strong>Date:</strong> {mailObject.Date}<br />
                           <strong>Subject:</strong> {subjectOriginal}</p>
                        <hr>
                        <pre style="font-family: sans-serif; white-space: pre-wrap;">{extractedBody}</pre>
                        <hr>
                        <p>Original message archived at <a href="{emailInfo.Url}">{emailInfo.Url}</a></p>
                        </body></html>
                        """;

        return new ForwardedEmailInfo
        {
            MessageId = emailInfo.MessageId,
            Subject = subject,
            SubjectOriginal = subjectOriginal,
            HtmlBody = bodyHtml,
            RawEmail = emailInfo.RawEmail,
            Resender = emailInfo.Resender
        };
    }

    /// <summary>
    /// Sending attachments using SES requires MIME message construction. I don't like it.
    ///
    /// https://docs.aws.amazon.com/ses/latest/dg/attachments.html
    /// </summary>
    /// <param name="forwarded"></param>
    /// <returns></returns>
    private string TransformToMimeString(ForwardedEmailInfo forwarded)
    {
        using var msg = new MimeMessage();
        msg.Subject = forwarded.Subject;
        msg.From.Add(MailboxAddress.Parse(forwarded.Resender));
        msg.To.Add(MailboxAddress.Parse(_config.EmailRecipient));

        var builder = new BodyBuilder { HtmlBody = forwarded.HtmlBody };

        // Attach original .eml
        var filename = forwarded.SubjectOriginal.ToSesAttachmentSafeFileName();

        builder.Attachments.Add($"{filename}.eml", forwarded.RawEmail, new ContentType("message", "rfc822"));
        msg.Body = builder.ToMessageBody();

        return msg.ToString();
    }
}