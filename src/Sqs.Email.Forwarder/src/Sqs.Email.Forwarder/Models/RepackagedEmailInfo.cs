namespace Sqs.Email.Forwarder.Models;

/// <summary>
/// The received email, repackaged in order to be forwarded to final inbox.
/// </summary>
internal record RepackagedEmailInfo
{
    public required string MessageId { get; init; }

    public required string SubjectOriginal { get; init; }

    public required string HtmlBody { get; init; }

    public required byte[] RawEmail { get; init; }

    public required string Resender { get; init; }

    public required string OriginalSenderEmail { get; init; }

    public required string OriginalRecipientEmail { get; init; }

    public required bool HasAttachments { get; init; }

    public required string OriginalMessageId { get; init; }

    public required DateTimeOffset OriginalDate { get; init; }

    public required string OriginalUrl { get; init; }
}
