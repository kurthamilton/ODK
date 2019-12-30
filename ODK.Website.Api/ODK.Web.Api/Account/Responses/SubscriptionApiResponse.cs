using System;

namespace ODK.Web.Api.Account.Responses
{
    public class SubscriptionApiResponse
    {
        public DateTime? ExpiryDate { get; set; }

        public Guid MemberId { get; set; }

        public int Type { get; set; }
    }
}
