namespace Sqs.Email.Forwarder.Models;

internal record StagedEmail
{
    public required string Subject { get; init; }

    public required string SenderEmail { get; init; }

    public required string RecipientEmail { get; init; }

    public required bool HasAttachments { get; init; }

    public required DateTimeOffset OriginalDate { get; init; }

    public required string PresignedUrl { get; init; }
}
