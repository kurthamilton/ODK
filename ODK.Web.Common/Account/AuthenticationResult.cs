using ODK.Core.Members;

namespace ODK.Web.Common.Account
{
    public class AuthenticationResult
    {
        public Member Member { get; set; }

        public bool Success { get; set; }
    }
}
