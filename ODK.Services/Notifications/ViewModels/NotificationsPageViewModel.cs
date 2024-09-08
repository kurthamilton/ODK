using ODK.Core.Members;

namespace ODK.Services.Notifications.ViewModels;

public class NotificationsPageViewModel
{
    public required bool IsAdmin { get; init; }

    public required IReadOnlyCollection<MemberNotificationSettings> Settings { get; init; }
}
