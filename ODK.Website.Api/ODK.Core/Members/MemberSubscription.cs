using System;
using ODK.Core.Utils;

namespace ODK.Core.Members
{
    public class MemberSubscription : IVersioned
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

        long IVersioned.Version => ExpiryDate == null ? 0 : DateUtils.DateVersion(ExpiryDate.Value);
    }
}
