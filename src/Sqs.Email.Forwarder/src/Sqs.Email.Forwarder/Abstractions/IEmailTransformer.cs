namespace Sqs.Email.Forwarder.Abstractions;

internal interface IEmailTransformer
{
    public Task<RepackagedEmailInfo> RepackageEmailAsync(ReceivedEmailInfo receivedEmailInfo);
}