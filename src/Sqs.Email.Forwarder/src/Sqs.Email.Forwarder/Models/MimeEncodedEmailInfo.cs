namespace Sqs.Email.Forwarder.Models;

internal record MimeEncodedEmailInfo
{
    public required string MessageId { get; init; }

    public required string Resender { get; init; }

    public required string MimeEncodedEmail { get; init; }
}