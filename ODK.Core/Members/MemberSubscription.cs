using ODK.Core.Utils;

namespace ODK.Core.Members;

public class MemberSubscription : IVersioned
{
    public DateTime? ExpiresUtc { get; set; }

    public Guid MemberId { get; set; }

    public SubscriptionType Type { get; set; }

    public byte[] Version => ExpiresUtc == null ? [] : BitConverter.GetBytes(DateUtils.DateVersion(ExpiresUtc.Value));
}
