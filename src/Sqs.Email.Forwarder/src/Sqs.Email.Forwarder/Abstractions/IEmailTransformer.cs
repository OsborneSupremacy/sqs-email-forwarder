namespace Sqs.Email.Forwarder.Abstractions;

internal interface IEmailTransformer
{
    public Task<MimeEncodedEmailInfo> RepackageAndTransformEmailAsync(ReceivedEmailInfo receivedEmailInfo);

    public Task<RepackagedEmailInfo> RepackageEmailAsync(ReceivedEmailInfo receivedEmailInfo);
}