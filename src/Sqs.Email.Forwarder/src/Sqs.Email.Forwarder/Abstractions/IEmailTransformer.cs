namespace Sqs.Email.Forwarder.Abstractions;

internal interface IEmailTransformer
{
    public Task<MimeEncodedEmailInfo> RepackageEmailAsync(ReceivedEmailInfo receivedEmailInfo);
}