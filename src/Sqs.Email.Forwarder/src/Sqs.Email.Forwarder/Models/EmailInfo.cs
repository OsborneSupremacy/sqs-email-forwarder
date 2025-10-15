namespace Sqs.Email.Forwarder.Models;

internal record EmailInfo
{
    public required string MessageId { get; init; }

    /// <summary>
    /// The email that will be used to re-send the email
    /// </summary>
    public required string Resender { get; init; }

    public required string Domain { get; init; }

    public required byte[] RawEmail { get; init; }

    public required string Url { get; init; }
}