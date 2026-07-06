namespace Sqs.Email.Forwarder.Models;

internal record ForwardedEmailInfo
{
    public required string MessageId { get; init; }

    public required string Subject { get; init; }

    public required string SubjectOriginal { get; init; }

    public required string HtmlBody { get; init; }

    public required byte[] RawEmail { get; init; }

    public required string Resender { get; init; }
}