using MimeKit;
using Sqs.Email.Forwarder.Models;

namespace Sqs.Email.Forwarder.Services;

internal class ExtractionService
{
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
            LocalPart = GetEmailLocalPart(senderEmail),
            NameAndAddress = senderInfo
        };
    }
    public MailboxInfo ExtractRelevantRecipientInfo(InternetAddressList recipientList, string domain)
    {
        var recipient = recipientList.Mailboxes
            .FirstOrDefault(r => r.Address.EndsWith($"@{domain}", StringComparison.OrdinalIgnoreCase));

        var recipientName = recipient?.Name ?? string.Empty;
        var recipientEmail = recipient?.Address ?? $"not-found@{domain}";
        var recipientInfo = !string.IsNullOrWhiteSpace(recipientName)
            ? $"{recipientName} <{recipientEmail}>"
            : recipientEmail;

        return new MailboxInfo
        {
            LocalPart = GetEmailLocalPart(recipientEmail),
            EmailAddress = recipientEmail,
            FriendlyName = recipientName,
            NameAndAddress = recipientInfo
        };
    }

    private static string GetEmailLocalPart(string emailAddress)
    {
        var atIndex = emailAddress.IndexOf('@');
        return atIndex <= 0 ? "unknown" : emailAddress[..atIndex];
    }
}