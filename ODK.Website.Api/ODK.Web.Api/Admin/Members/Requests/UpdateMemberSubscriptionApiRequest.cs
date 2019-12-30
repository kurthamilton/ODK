using System;
using ODK.Core.Members;

namespace ODK.Web.Api.Admin.Members.Requests
{
    public class UpdateMemberSubscriptionApiRequest
    {
        public DateTime? ExpiryDate { get; set; }

        public SubscriptionType Type { get; set; }
    }
}
