namespace Sqs.Email.Forwarder.Models;

internal record Config
{
    public required string[] MailBuckets { get; init; }

    public required string[] EmailSenders { get; init; }

    public required string AwsRegion { get; init; }

    public required string EmailRecipient { get; init; }

    public required string StagingBucket { get; init; }
}