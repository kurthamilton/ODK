using ODK.Core.Notifications;

namespace ODK.Core.Members;

public class MemberNotificationSettings
{
    public bool Disabled { get; set; }

    public Guid MemberId { get; set; }

    public NotificationType NotificationType { get; set; }
}
