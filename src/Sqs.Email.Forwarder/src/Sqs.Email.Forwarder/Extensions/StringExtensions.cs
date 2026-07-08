using System.Text.RegularExpressions;

namespace Sqs.Email.Forwarder.Extensions;

internal static partial class StringExtensions
{
    [GeneratedRegex("[^a-zA-Z0-9]")]
    private static partial Regex NonAlphaNumericRegex();

    extension(string input)
    {
        /// <summary>
        /// Gets the local part of an email address (the part before the '@' symbol).
        /// </summary>
        /// <returns></returns>
        public string GetEmailLocalPart()
        {
            var atIndex = input.IndexOf('@');
            return atIndex <= 0 ? "unknown" : input[..atIndex];
        }

        public string GetEmailDomain()
        {
            var atIndex = input.IndexOf('@');
            if (atIndex < 0 || atIndex == input.Length - 1)
                return "@unknown.com";
            return input[(atIndex + 1)..];
        }

        public string ToSesAttachmentSafeFileName()
        {
            var fileName = NonAlphaNumericRegex().Replace(input, string.Empty);
            return fileName.Length > 50 ? fileName[..50] : fileName;
        }

        public string ToMessageIdHeaderValue()
        {
            var hasLeadingBracket = input.StartsWith('<');
            var hasTrailingBracket = input.EndsWith('>');

            return hasLeadingBracket && hasTrailingBracket
                ? input
                : $"<{input}>";
        }
    }
}