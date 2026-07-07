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

    public async Task<MimeEncodedEmailInfo> RepackageEmailAsync(ReceivedEmailInfo receivedEmailInfo)
    {
        using var mailObject = await GetMailObjectAsync(receivedEmailInfo)
            .ConfigureAwait(false);

        var repackaged = RepackagedEmail(receivedEmailInfo, mailObject);

        return MimeEncodeEmail(repackaged);
    }

    private async Task<MimeMessage> GetMailObjectAsync(ReceivedEmailInfo receivedEmailInfo)
    {
        await using var messageStream = new MemoryStream(receivedEmailInfo.RawEmail);
        return await MimeMessage
            .LoadAsync(messageStream)
            .ConfigureAwait(false);
    }

    private RepackagedEmailInfo RepackagedEmail(ReceivedEmailInfo receivedEmailInfo, MimeMessage mailObject)
    {
        var subjectOriginal = mailObject.Subject ?? "(no subject)";

        var sender = _extractionService.ExtractSenderInfo(mailObject.From);
        var recipient = _extractionService.ExtractRelevantRecipientInfo(mailObject.To, receivedEmailInfo.Domain);

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
                            <div style="font-family: sans-serif;">{extractedBody}</div>
                        <hr>
                        <p>Original message archived at <a href="{receivedEmailInfo.Url}">{receivedEmailInfo.Url}</a></p>
                        </body></html>
                        """;

        return new RepackagedEmailInfo
        {
            MessageId = receivedEmailInfo.MessageId,
            Subject = subject,
            SubjectOriginal = subjectOriginal,
            HtmlBody = bodyHtml,
            RawEmail = receivedEmailInfo.RawEmail,
            Resender = receivedEmailInfo.Resender
        };
    }

    /// <summary>
    /// Sending attachments using SES requires MIME message construction. I don't like it.
    ///
    /// https://docs.aws.amazon.com/ses/latest/dg/attachments.html
    /// </summary>
    /// <param name="repackaged"></param>
    /// <returns></returns>
    private MimeEncodedEmailInfo MimeEncodeEmail(RepackagedEmailInfo repackaged)
    {
        using var msg = new MimeMessage();
        msg.Subject = repackaged.Subject;
        msg.From.Add(MailboxAddress.Parse(repackaged.Resender));
        msg.To.Add(MailboxAddress.Parse(_config.EmailRecipient));

        var builder = new BodyBuilder { HtmlBody = repackaged.HtmlBody };

        // Attach original .eml
        var filename = repackaged.SubjectOriginal.ToSesAttachmentSafeFileName();

        builder.Attachments.Add($"{filename}.eml", repackaged.RawEmail, new ContentType("message", "rfc822"));
        msg.Body = builder.ToMessageBody();

        return new MimeEncodedEmailInfo
        {
            MessageId = repackaged.MessageId,
            Resender = repackaged.Resender,
            MimeEncodedEmail = msg.ToString()
        };
    }
}