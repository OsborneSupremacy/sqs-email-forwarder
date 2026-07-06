namespace Sqs.Email.Forwarder.Abstractions;

internal interface IEmailProvider
{
    public Task<ReceivedEmailInfo> GetReceivedEmailAsync(string messageId);
}