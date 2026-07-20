using MimeKit;

namespace Sqs.Email.Forwarder.Services;

internal class EmailTransformer : IEmailTransformer
{
    private readonly ILogger<EmailTransformer> _logger;

    private readonly IExtractionService  _extractionService;

    private readonly EmailMimeComposer _emailMimeComposer;

    public EmailTransformer(
        ILogger<EmailTransformer> logger,
        EmailMimeComposer emailMimeComposer,
        IExtractionService extractionService
        )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _emailMimeComposer = emailMimeComposer ?? throw new ArgumentNullException(nameof(emailMimeComposer));
        _extractionService = extractionService ?? throw new ArgumentNullException(nameof(extractionService));
    }

    public async Task<MimeEncodedEmailInfo> RepackageAndTransformEmailAsync(ReceivedEmailInfo receivedEmailInfo)
    {
        var repackaged = await RepackageEmailAsync(receivedEmailInfo);
        return _emailMimeComposer.Compose(repackaged);
    }

    public async Task<RepackagedEmailInfo> RepackageEmailAsync(ReceivedEmailInfo receivedEmailInfo)
    {
        using var mailObject = await GetMailObjectAsync(receivedEmailInfo)
            .ConfigureAwait(false);

        var repackaged = RepackageEmail(receivedEmailInfo, mailObject);

        return repackaged;
    }

    private async Task<MimeMessage> GetMailObjectAsync(ReceivedEmailInfo receivedEmailInfo)
    {
        await using var messageStream = new MemoryStream(receivedEmailInfo.RawEmail);
        return await MimeMessage
            .LoadAsync(messageStream)
            .ConfigureAwait(false);
    }

    private RepackagedEmailInfo RepackageEmail(ReceivedEmailInfo receivedEmailInfo, MimeMessage mailObject)
    {
        var subjectOriginal = mailObject.Subject ?? "(no subject)";

        var sender = _extractionService.ExtractSenderInfo(mailObject.From);
        var recipient = _extractionService
            .ExtractRelevantRecipientInfo(receivedEmailInfo.Domain, mailObject.To, mailObject.Cc, mailObject.Bcc);

        var subject = BuildForwardSubject(subjectOriginal, sender, recipient);
        var bodyHtml = BuildForwardHtmlBody(receivedEmailInfo, mailObject, sender, subjectOriginal);

        return new RepackagedEmailInfo
        {
            MessageId = receivedEmailInfo.MessageId,
            Subject = subject,
            SubjectOriginal = subjectOriginal,
            HtmlBody = bodyHtml,
            RawEmail = receivedEmailInfo.RawEmail,
            Resender = receivedEmailInfo.Resender,
            OriginalSenderEmail = sender.EmailAddress,
            OriginalRecipientEmail = recipient.EmailAddress,
            OriginalMessageId = mailObject.MessageId ?? string.Empty,
            OriginalDate = mailObject.Date,
            OriginalUrl = receivedEmailInfo.Url
        };
    }

    private static string BuildForwardSubject(string subjectOriginal, MailboxInfo sender, MailboxInfo recipient)
        => $"[{sender.FriendlyName}]➡️[{recipient.LocalPart}] {subjectOriginal}";

    private string BuildForwardHtmlBody(ReceivedEmailInfo receivedEmailInfo, MimeMessage mailObject, MailboxInfo sender, string subjectOriginal)
    {
        var extractedBody = _extractionService.ExtractBody(mailObject);

        var e = new System.Text.StringBuilder();
        e.AppendLine($"""
                      <html>
                      <head>
                          <meta charset="utf-8" />
                      </head>
                      <body>
                          <p><strong>Forwarded message:</strong></p>
                          <p>
                              <strong>From:</strong> {sender.FriendlyName} | {sender.EmailAddress}<br />
                              <strong>To:</strong> {mailObject.To.ToDisplayNamesAndEmails()}<br />
                      """);

        if (mailObject.Cc.Count > 0)
            e.AppendLine($"<strong>CC:</strong> {mailObject.Cc.ToDisplayNamesAndEmails()}<br />");

        e.AppendLine($"""
                                  <strong>Date:</strong> {mailObject.Date}<br />
                                  <strong>Subject:</strong> {subjectOriginal}
                              </p>
                              <hr>
                                  <div style="font-family: sans-serif;">{extractedBody}</div>
                              <hr>
                              <p>Original message archived at <a href="{receivedEmailInfo.Url}">{receivedEmailInfo.Url}</a></p>
                          </body>
                      </html>
                      """);



        return e.ToString();
    }
}
