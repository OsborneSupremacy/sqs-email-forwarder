using System.Collections.Immutable;
using MimeKit;

namespace Sqs.Email.Forwarder.Services;

internal class ExtractionService : IExtractionService
{
    private readonly ILogger<ExtractionService> _logger;

    public ExtractionService(ILogger<ExtractionService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string ExtractSesMessageId(string sqsBody)
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

    public string ExtractBody(MimeMessage mailObject)
    {
        if (!string.IsNullOrWhiteSpace(mailObject.HtmlBody))
            return mailObject.HtmlBody;

        if (string.IsNullOrWhiteSpace(mailObject.TextBody))
            return "<p>(No readable message body found)</p>";

        var encoded = WebUtility.HtmlEncode(mailObject.TextBody);
        return encoded.Replace("\r\n", "<br />").Replace("\n", "<br />");
    }

    public MailboxInfo ExtractSenderInfo(InternetAddressList senderList)
    {
        var sender = senderList.Mailboxes.FirstOrDefault();

        var senderName = sender?.Name ?? string.Empty;
        var senderEmail = sender?.Address ?? "unknown@unknown.com";
        var senderInfo = !string.IsNullOrWhiteSpace(senderName)
            ? $"{senderName} <{senderEmail}>"
            : senderEmail;

        return new MailboxInfo
        {
            EmailAddress = senderEmail,
            FriendlyName = string.IsNullOrWhiteSpace(senderName) ? senderEmail : senderName,
            LocalPart = senderEmail.GetEmailLocalPart(),
            NameAndAddress = senderInfo
        };
    }

    public MailboxInfo ExtractRelevantRecipientInfo(string domain, params ImmutableList<InternetAddressList> recipientLists)
    {
        MailboxAddress? recipient = null;

        foreach (var recipientList in recipientLists)
        {
            recipient = recipientList.Mailboxes
                .FirstOrDefault(r => r.Address.EndsWith($"@{domain}", StringComparison.OrdinalIgnoreCase));
            if(recipient is not null) break;
        }

        var recipientName = recipient?.Name ?? string.Empty;
        var recipientEmail = recipient?.Address ?? $"not-found@{domain}";
        var recipientInfo = !string.IsNullOrWhiteSpace(recipientName)
            ? $"{recipientName} <{recipientEmail}>"
            : recipientEmail;

        return new MailboxInfo
        {
            LocalPart = recipientEmail.GetEmailLocalPart(),
            EmailAddress = recipientEmail,
            FriendlyName = recipientName,
            NameAndAddress = recipientInfo
        };
    }
}