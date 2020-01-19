using System;
using System.Collections.Generic;

namespace ODK.Web.Common.Account.Responses
{
    public class AuthenticationTokenApiResponse
    {
        public string AccessToken { get; set; }

        public IEnumerable<Guid> AdminChapterIds { get; set; }

        public Guid ChapterId { get; set; }

        public Guid MemberId { get; set; }

        public bool? MembershipDisabled { get; set; }

        public string RefreshToken { get; set; }

        public DateTime? SubscriptionExpiryDate { get; set; }

        public bool? SuperAdmin { get; set; }
    }
}
