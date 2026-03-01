namespace ODK.Core.Notifications;

public static class NotificationGroupTypeExtensions
{
    public static bool ForAdmins(this NotificationGroupType group) => group switch
    {
        NotificationGroupType.Members => true,
        _ => false
    };

    public static IEnumerable<NotificationType> Types(this NotificationGroupType group)
        => Enum.GetValues<NotificationType>()
            .Where(x => x != NotificationType.None && x.Group() == group);
}