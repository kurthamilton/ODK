using ODK.Core.Utils;

namespace ODK.Core.Members;

public class MemberSubscription : IVersioned
{
    public DateTime? ExpiresUtc { get; set; }

    public bool IsExpired() => ExpiresUtc < DateTime.UtcNow;

    public MemberChapter MemberChapter { get; set; } = null!;

    public Guid MemberChapterId { get; set; }

    public DateTime? ReminderEmailSentUtc { get; set; }

    public SubscriptionType Type { get; set; }

    public byte[] Version => ExpiresUtc == null ? [] : BitConverter.GetBytes(DateUtils.DateVersion(ExpiresUtc.Value));
}
