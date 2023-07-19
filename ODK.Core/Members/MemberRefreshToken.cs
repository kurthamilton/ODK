using System;

namespace ODK.Core.Members
{
    public class MemberRefreshToken
    {
        public MemberRefreshToken(Guid id, Guid memberId, DateTime expires, string refreshToken)
        {
            Expires = expires;
            Id = id;
            MemberId = memberId;
            RefreshToken = refreshToken;
        }

        public DateTime Expires { get; }

        public Guid Id { get; }

        public Guid MemberId { get; }

        public string RefreshToken { get; }
    }
}
