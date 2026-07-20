namespace Sqs.Email.Forwarder.Models;

internal record StagedEmail
{
    public required string MessageId { get; init; }

    public required string OriginalSubject { get; init; }

    public required string OriginalSenderEmail { get; init; }

    public required string OriginalRecipientEmail { get; init; }

    public required DateTimeOffset OriginalDate { get; init; }

    public required string Domain { get; init; }

    public required string OriginalUrl { get; init; }

    public required string PresignedUrl { get; init; }
}
