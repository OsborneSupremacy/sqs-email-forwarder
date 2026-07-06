using System.Text.RegularExpressions;

namespace Sqs.Email.Forwarder.Extensions;

internal static partial class StringExtensions
{
    [GeneratedRegex("[^a-zA-Z0-9]")]
    private static partial Regex NonAlphaNumericRegex();

    extension(string emailAddress)
    {
        /// <summary>
        /// Gets the local part of an email address (the part before the '@' symbol).
        /// </summary>
        /// <returns></returns>
        public string GetEmailLocalPart()
        {
            var atIndex = emailAddress.IndexOf('@');
            return atIndex <= 0 ? "unknown" : emailAddress[..atIndex];
        }

        public string GetEmailDomain()
        {
            var atIndex = emailAddress.IndexOf('@');
            if (atIndex < 0 || atIndex == emailAddress.Length - 1)
                return "@unknown.com";
            return emailAddress[(atIndex + 1)..];
        }

        public string ToSesAttachmentSafeFileName()
        {
            var fileName = NonAlphaNumericRegex().Replace(emailAddress, string.Empty);
            return fileName.Length > 50 ? fileName[..50] : fileName;
        }
    }
}