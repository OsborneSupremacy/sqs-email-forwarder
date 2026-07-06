namespace Sqs.Email.Forwarder.Abstractions;

internal interface IProcessor
{
    public Task ProcessMessageAsync(SQSEvent.SQSMessage message);
}