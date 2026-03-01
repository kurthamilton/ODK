using ODK.Core.Notifications;

namespace ODK.Core.Members;

public class MemberChapterNotificationSettings : IDatabaseEntity
{
    public bool Disabled { get; set; }

    public Guid Id { get; set; }

    public Guid MemberChapterId { get; set; }

    public NotificationType NotificationType { get; set; }
}