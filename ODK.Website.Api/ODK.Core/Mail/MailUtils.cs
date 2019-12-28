using System.Text.RegularExpressions;

namespace ODK.Core.Mail
{
    public static class MailUtils
    {
        private static readonly Regex EmailAddressRegex = new Regex("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$", RegexOptions.Compiled);

        public static bool ValidEmailAddress(string emailAddress)
        {
            return EmailAddressRegex.IsMatch(emailAddress);
        }
    }
}
