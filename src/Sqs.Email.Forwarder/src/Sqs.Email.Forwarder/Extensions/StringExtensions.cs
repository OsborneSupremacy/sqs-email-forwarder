namespace Sqs.Email.Forwarder.Extensions;

internal static class StringExtensions
{
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
    }
}