using System;

namespace ODK.Web.Api.Account
{
    public class AuthenticationTokenResponse
    {
        public string AccessToken { get; set; }

        public Guid ChapterId { get; set; }

        public Guid MemberId { get; set; }

        public string RefreshToken { get; set; }
    }
}
