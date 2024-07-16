using ODK.Core.Utils;

namespace ODK.Core.Members;

public class MemberSubscription : IVersioned
{
    public DateTime? ExpiryDate { get; set; }

    public Guid MemberId { get; set; }

    public SubscriptionType Type { get; set; }

    public byte[] Version => ExpiryDate == null ? [] : BitConverter.GetBytes(DateUtils.DateVersion(ExpiryDate.Value));
}
