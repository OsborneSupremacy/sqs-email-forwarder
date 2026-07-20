namespace Sqs.Email.Forwarder.Abstractions;

/// <summary>
/// Composes and sends a summary email for a batch of staged emails.
/// </summary>
internal interface IAggregator
{
    public Task EmailAggregateAsync(ImmutableList<StagedEmail> stagedEmails);
}
