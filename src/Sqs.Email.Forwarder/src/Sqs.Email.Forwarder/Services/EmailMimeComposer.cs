using System.Diagnostics.CodeAnalysis;
using MimeKit;

namespace Sqs.Email.Forwarder.Services;

internal class EmailMimeComposer
{
    private readonly Config _config;

    public EmailMimeComposer(Config config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Sending attachments using SES requires MIME message construction. I don't like it.
    ///
    /// https://docs.aws.amazon.com/ses/latest/dg/attachments.html
    /// </summary>
    /// <param name="repackaged"></param>
    /// <returns></returns>
    [SuppressMessage("Performance", "CA1865:Use char overload")]
    public MimeEncodedEmailInfo Compose(RepackagedEmailInfo repackaged)
    {
        using var msg = new MimeMessage();
        msg.Subject = repackaged.Subject;
        msg.From.Add(MailboxAddress.Parse(repackaged.Resender));
        msg.To.Add(MailboxAddress.Parse(_config.EmailRecipient));
        ApplyForwardHeaders(msg, repackaged);
        msg.Body = BuildForwardBody(repackaged);

        return new MimeEncodedEmailInfo
        {
            MessageId = repackaged.MessageId,
            Resender = repackaged.Resender,
            MimeEncodedEmail = msg.ToString()
        };
    }

    private static void ApplyForwardHeaders(MimeMessage msg, RepackagedEmailInfo repackaged)
    {
        msg.ReplyTo.Add(MailboxAddress.Parse(repackaged.OriginalSenderEmail));

        if (!string.IsNullOrWhiteSpace(repackaged.OriginalMessageId))
        {
            var formattedMessageId = repackaged.OriginalMessageId.ToMessageIdHeaderValue();
            msg.Headers[HeaderId.InReplyTo] = formattedMessageId;
            msg.Headers[HeaderId.References] = formattedMessageId;
        }

        if (repackaged.OriginalDate != default)
            msg.Headers["X-Original-Date"] = repackaged.OriginalDate.ToString("R");
    }

    private static Multipart BuildForwardBody(RepackagedEmailInfo repackaged)
    {
        var htmlPart = new TextPart("html")
        {
            Text = repackaged.HtmlBody,
            ContentTransferEncoding = ContentEncoding.QuotedPrintable
        };
        htmlPart.ContentType.Charset = "utf-8";

        // Attach original .eml as a separate MIME part to preserve the source message.
        var filename = repackaged.SubjectOriginal.ToSesAttachmentSafeFileName();
        var attachmentPart = new MimePart("message", "rfc822")
        {
            FileName = $"{filename}.eml",
            Content = new MimeContent(new MemoryStream(repackaged.RawEmail)),
            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment)
        };

        return new Multipart("mixed")
        {
            htmlPart,
            attachmentPart
        };
    }
}