using System;
using System.Collections.Generic;
using System.Linq;

namespace ODK.Services.Authentication
{
    public class AuthenticationToken
    {
        public AuthenticationToken(Guid memberId, Guid chapterId, string accessToken, string refreshToken,
            IEnumerable<Guid> adminChapterIds, bool superAdmin, DateTime? subscriptionExpiryDate)
        {
            AccessToken = accessToken;
            AdminChapterIds = adminChapterIds?.ToArray();
            ChapterId = chapterId;
            MemberId = memberId;
            RefreshToken = refreshToken;
            SubscriptionExpiryDate = subscriptionExpiryDate;
            SuperAdmin = superAdmin;
        }

        public string AccessToken { get; }

        public IReadOnlyCollection<Guid> AdminChapterIds { get; }

        public Guid ChapterId { get; }

        public Guid MemberId { get; }

        public string RefreshToken { get; }

        public DateTime? SubscriptionExpiryDate { get; }

        public bool SuperAdmin { get; }
    }
}
