namespace Sqs.Email.Forwarder.Abstractions;

internal interface IEmailSender
{
    public Task SendEmailAsync(MimeEncodedEmailInfo mimeEncodedEmailInfo);
}