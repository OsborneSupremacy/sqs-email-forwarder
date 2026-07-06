namespace Sqs.Email.Forwarder.Abstractions;

internal interface IEmailTransformer
{
    public Task<string> TransformToForwardedEmailAsync(EmailInfo emailInfo);
}