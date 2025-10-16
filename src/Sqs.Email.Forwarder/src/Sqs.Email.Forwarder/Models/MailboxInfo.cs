namespace Sqs.Email.Forwarder.Models;

internal record MailboxInfo
{
    public required string FriendlyName { get; init; }

    /// <summary>
    /// The part before the @ in an email address
    /// </summary>
    public required string LocalPart { get; init; }

    public required string EmailAddress { get; init; }

    public required string NameAndAddress { get; init; }
}