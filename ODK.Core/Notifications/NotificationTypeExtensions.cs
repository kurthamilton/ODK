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
}
