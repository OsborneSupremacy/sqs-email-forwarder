namespace Sqs.Email.Forwarder.Models;

/// <summary>
/// The email message as it was originally received in S3 / SNS
/// </summary>
internal record ReceivedEmailInfo
{
    public required string MessageId { get; init; }

    public required string Domain { get; init; }

    public required byte[] RawEmail { get; init; }

    public required string Url { get; init; }
}