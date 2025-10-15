namespace Sqs.Email.Forwarder.Models;

internal record MailboxInfo
{
    public required string FriendlyName { get; init; }

    public required string EmailAddress { get; init; }

    public required string NameAndAddress { get; init; }
}