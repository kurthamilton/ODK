using System;
using ODK.Core.Members;

namespace ODK.Services.Members
{
    public class UpdateMemberSubscription
    {
        public DateTime? ExpiryDate { get; set; }

        public SubscriptionType Type { get; set; }
    }
}
