namespace ODK.Services.Notifications.ViewModels;

public class NotificationListContentViewModel
{
    public string? ItemClass { get; init; }

    public required UnreadNotificationsViewModel Notifications { get; init; }
}