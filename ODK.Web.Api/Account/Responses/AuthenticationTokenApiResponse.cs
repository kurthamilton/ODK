using System;

namespace ODK.Web.Api.Account.Responses
{
    public class AuthenticationTokenApiResponse
    {
        public string AccessToken { get; set; }

        public Guid ChapterId { get; set; }

        public Guid MemberId { get; set; }

        public string RefreshToken { get; set; }
    }
}
