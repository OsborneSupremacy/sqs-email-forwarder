namespace Sqs.Email.Forwarder.Abstractions;

internal interface IEmailSender
{
    public Task SendEmailAsync(
        EmailInfo emailInfo,
        string forwardedEmail
    );
}