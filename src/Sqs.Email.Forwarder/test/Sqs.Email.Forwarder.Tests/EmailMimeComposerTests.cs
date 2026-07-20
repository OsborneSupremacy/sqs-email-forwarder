using System.Text;
using MimeKit;

namespace Sqs.Email.Forwarder.Tests;

public class EmailMimeComposerTests
{
    [Fact]
    public void Compose_GivenRepackagedEmail_BuildsUtf8HtmlBodyWithThreadingHeadersAndAttachment()
    {
        // arrange
        var config = new Config
        {
            MailBuckets = ["bucket"],
            EmailSenders = ["sender@example.com"],
            AwsRegion = "us-east-1",
            EmailRecipient = "final@example.com",
            StagingBucket = "bro-ses-inbox-staged"
        };

        var sut = new EmailMimeComposer(config);
        var repackaged = new RepackagedEmailInfo
        {
            MessageId = "message-123",
            Subject = "Forwarded subject",
            SubjectOriginal = "Original Subject",
            HtmlBody = "<html><body><p>Hello world</p></body></html>",
            RawEmail = "From: original@example.com\r\nTo: final@example.com\r\nSubject: Original Subject\r\n\r\nBody"u8.ToArray(),
            Resender = "resender@example.com",
            OriginalSenderEmail = "original@example.com",
            OriginalRecipientEmail = "recipient@example.com",
            HasAttachments = true,
            OriginalMessageId = "original-message-id",
            OriginalDate = new DateTimeOffset(2026, 07, 07, 12, 34, 56, TimeSpan.Zero),
            OriginalUrl = string.Empty
        };

        // act
        var result = sut.Compose(repackaged);

        var message = MimeMessage.Load(new MemoryStream(Encoding.UTF8.GetBytes(result.MimeEncodedEmail)));

        // assert
        message.Subject.Should().Be("Forwarded subject");
        message.From.Mailboxes.Should().ContainSingle(mailbox => mailbox.Address == "resender@example.com");
        message.To.Mailboxes.Should().ContainSingle(mailbox => mailbox.Address == "final@example.com");
        message.ReplyTo.Mailboxes.Should().ContainSingle(mailbox => mailbox.Address == "original@example.com");
        message.Headers[HeaderId.InReplyTo].Should().Be("<original-message-id>");
        message.Headers[HeaderId.References].Should().Be("<original-message-id>");
        message.Headers["X-Original-Date"].Should().Be("Tue, 07 Jul 2026 12:34:56 GMT");

        message.Body.Should().BeOfType<Multipart>();
        var multipart = (Multipart)message.Body;
        multipart.Should().HaveCount(2);

        var htmlPart = multipart.OfType<TextPart>().Single(part => part.IsHtml);
        htmlPart.ContentType.Charset.Should().Be("utf-8");
        htmlPart.ContentTransferEncoding.Should().Be(ContentEncoding.QuotedPrintable);
        htmlPart.Text.Should().Contain("Hello world");

        var attachment = multipart[1];
        attachment.ContentType.MediaType.Should().Be("message");
        attachment.ContentType.MediaSubtype.Should().Be("rfc822");
    }
}
