namespace Sqs.Email.Forwarder.Models;

/// <summary>
/// The received email, repackaged in order to be forwarded to final inbox.
/// </summary>
internal record RepackagedEmailInfo
{
    public required string MessageId { get; init; }

    public required string Subject { get; init; }

    public required string HtmlBody { get; init; }

    public required string SenderEmail { get; init; }

    public required string RecipientEmail { get; init; }

    public required bool HasAttachments { get; init; }

    public required DateTimeOffset OriginalDate { get; init; }
}
