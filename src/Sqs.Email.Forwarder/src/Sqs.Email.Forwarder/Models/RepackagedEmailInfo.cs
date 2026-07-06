namespace Sqs.Email.Forwarder.Models;

/// <summary>
/// The received email, repackaged in order to be forwarded to final inbox.
/// </summary>
internal record RepackagedEmailInfo
{
    public required string MessageId { get; init; }

    public required string Subject { get; init; }

    public required string SubjectOriginal { get; init; }

    public required string HtmlBody { get; init; }

    public required byte[] RawEmail { get; init; }

    public required string Resender { get; init; }
}