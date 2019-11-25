using System;

namespace ODK.Services.Authentication
{
    public class AuthenticationToken
    {
        public AuthenticationToken(Guid memberId, Guid chapterId, string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            ChapterId = chapterId;
            MemberId = memberId;
            RefreshToken = refreshToken;
        }

        public string AccessToken { get; }

        public Guid ChapterId { get; }

        public Guid MemberId { get; }

        public string RefreshToken { get; }
    }
}
