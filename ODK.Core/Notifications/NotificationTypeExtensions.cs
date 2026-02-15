namespace ODK.Core.Notifications;

public static class NotificationTypeExtensions
{
    public static bool ForAdmins(this NotificationType type) => type switch
    {
        NotificationType.ConversationOwnerMessage => true,
        NotificationType.ChapterContactMessage => true,
        NotificationType.NewMember => true,
        _ => false
    };

    public static NotificationGroupType Group(this NotificationType type) => type switch
    {
        NotificationType.ChapterContactMessage => NotificationGroupType.Messages,
        NotificationType.ConversationAdminMessage => NotificationGroupType.Messages,
        NotificationType.ConversationOwnerMessage => NotificationGroupType.Messages,
        NotificationType.EventWaitlistPromotion => NotificationGroupType.Events,
        NotificationType.NewEvent => NotificationGroupType.Events,
        NotificationType.NewMember => NotificationGroupType.Members,
        _ => NotificationGroupType.None
    };
}