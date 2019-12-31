using System;
using System.Collections.Generic;
using System.Linq;

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

        public AuthenticationToken(Guid memberId, Guid chapterId, string accessToken, string refreshToken,
            IEnumerable<Guid> adminChapterIds, bool superAdmin, DateTime? subscriptionExpiryDate, bool membershipActive)
            : this(memberId, chapterId, accessToken, refreshToken)
        {
            AdminChapterIds = adminChapterIds?.ToArray();         
            MembershipActive = membershipActive;
            SubscriptionExpiryDate = subscriptionExpiryDate;
            SuperAdmin = superAdmin;
        }

        public string AccessToken { get; }

        public IReadOnlyCollection<Guid> AdminChapterIds { get; }

        public Guid ChapterId { get; }

        public Guid MemberId { get; }

        public bool MembershipActive { get; }

        public string RefreshToken { get; }

        public DateTime? SubscriptionExpiryDate { get; }

        public bool SuperAdmin { get; }
    }
}
