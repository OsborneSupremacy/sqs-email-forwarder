namespace Sqs.Email.Forwarder.Abstractions;

internal interface IEmailProvider
{
    public Task<EmailInfo> GetEmailAsync(string messageId);
}