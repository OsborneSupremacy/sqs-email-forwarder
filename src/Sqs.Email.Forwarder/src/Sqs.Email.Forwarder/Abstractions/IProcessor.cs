namespace Sqs.Email.Forwarder.Abstractions;

internal interface IProcessor
{
    /// <summary>
    /// Processes a batch of SQS messages.
    /// </summary>
    /// <param name="messages">The messages to process.</param>
    /// <returns>The IDs of successfully-processed messages.</returns>
    Task<ImmutableList<string>> ProcessMessagesAsync(ImmutableList<SQSEvent.SQSMessage> messages);
}