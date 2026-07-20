using System.Text;
using MimeKit;

namespace Sqs.Email.Forwarder.Services;

/// <summary>
/// <inheritdoc/>
/// </summary>
internal class Aggregator : IAggregator
{
    private readonly IEmailSender _emailSender;

    private readonly Config _config;

    public Aggregator(IEmailSender emailSender, Config config)
    {
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task EmailAggregateAsync(ImmutableList<StagedEmail> stagedEmails)
    {
        var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, localTimeZone);

        var messageId = Guid.NewGuid().ToString("N");
        var resender = _config.EmailSenders[0];

        using var message = new MimeMessage();
        message.Subject = $"Email Digest for {localTime:yyyy-MM-dd hh:mm tt} ({stagedEmails.Count})";
        message.From.Add(MailboxAddress.Parse(resender));
        message.To.Add(MailboxAddress.Parse(_config.EmailRecipient));
        message.Body = new TextPart("html")
        {
            Text = BuildHtmlBody(stagedEmails),
            ContentTransferEncoding = ContentEncoding.QuotedPrintable
        };

        await _emailSender
            .SendEmailAsync(new MimeEncodedEmailInfo
            {
                MessageId = messageId,
                Resender = resender,
                MimeEncodedEmail = message.ToString()
            })
            .ConfigureAwait(false);
    }

    private static string BuildHtmlBody(ImmutableList<StagedEmail> stagedEmails)
    {
        var html = new StringBuilder();
        html.AppendLine("<html><head><meta charset=\"utf-8\" /></head><body style=\"font-family: sans-serif;\">");
        html.AppendLine($"<h1>Received emails ({stagedEmails.Count})</h1>");

        foreach (var stagedEmail in stagedEmails)
        {
            html.AppendLine("<div style=\"margin-bottom: 24px; padding-bottom: 16px; border-bottom: 1px solid #ddd;\">");
            html.AppendLine($"<h2 style=\"font-size: 18px;\">{Encode(stagedEmail.OriginalSubject)}</h2>");
            html.AppendLine($"<div><strong>Sender:</strong> {Encode(stagedEmail.OriginalSenderEmail)}</div>");
            html.AppendLine($"<div><strong>Recipient:</strong> {Encode(stagedEmail.OriginalRecipientEmail)}</div>");
            html.AppendLine($"<div><strong>Original date:</strong> {Encode(stagedEmail.OriginalDate.ToString("yyyy-MM-dd HH:mm zzz"))}</div>");
            html.AppendLine($"<div style=\"margin-top: 12px;\"><a href=\"{Encode(stagedEmail.PresignedUrl)}\">View staged email</a></div>");
            html.AppendLine("</div>");
        }

        html.AppendLine("</body></html>");
        return html.ToString();
    }

    private static string Encode(string value) => WebUtility.HtmlEncode(value);
}
