using System;

namespace ODK.Core.Members
{
    public class MemberSubscriptionRecord
    {
        public MemberSubscriptionRecord(Guid memberId, SubscriptionType type, DateTime purchaseDate)
        {
            MemberId = memberId;
            PurchaseDate = purchaseDate;
            Type = type;
        }

        public Guid MemberId { get; }

        public DateTime PurchaseDate { get; }

        public SubscriptionType Type { get; }
    }
}
