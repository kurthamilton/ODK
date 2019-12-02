using System;

namespace ODK.Core.Members
{
    public class MemberSubscription
    {
        public MemberSubscription(Guid memberId, SubscriptionType type, DateTime? expiryDate)
        {
            ExpiryDate = expiryDate;
            MemberId = memberId;
            Type = type;
        }

        public DateTime? ExpiryDate { get; }

        public Guid MemberId { get; }

        public SubscriptionType Type { get; }
    }
}
