using MimeKit;

namespace Sqs.Email.Forwarder.Extensions;

internal static class InternetAddressListExtensions
{
    private const string UnknownEmailAddress = "unknown@unknown.com";

    extension(InternetAddressList addresses)
    {
        public string ToDisplayNamesAndEmails()
        {
            ArgumentNullException.ThrowIfNull(addresses);

            if (addresses.Count == 0)
                return string.Empty;

            return string.Join(", ", addresses.Select(FormatAddress));
        }

        private static string FormatAddress(InternetAddress address)
            => address switch
            {
                MailboxAddress mailbox => FormatMailbox(mailbox),
                GroupAddress group => FormatNamedAddressWithoutMailbox(group.Name),
                _ => UnknownEmailAddress
            };

        private static string FormatMailbox(MailboxAddress mailbox)
        {
            var emailAddress = mailbox.Address?.Trim();
            var displayName = NormalizeName(mailbox.Name);

            if (string.IsNullOrWhiteSpace(emailAddress))
                emailAddress = UnknownEmailAddress;

            if (string.IsNullOrWhiteSpace(displayName))
                return emailAddress;

            return $"{displayName} | {emailAddress}";
        }

        private static string FormatNamedAddressWithoutMailbox(string? name)
        {
            var displayName = NormalizeName(name);

            return string.IsNullOrWhiteSpace(displayName)
                ? UnknownEmailAddress
                : $"{displayName} | {UnknownEmailAddress}";
        }

        private static string NormalizeName(string? name)
            => string.IsNullOrWhiteSpace(name)
                ? string.Empty
                : name.Replace("\"", string.Empty).Trim();
    }
}