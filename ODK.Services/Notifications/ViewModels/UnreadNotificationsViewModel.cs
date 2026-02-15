using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Platforms;

namespace ODK.Services.Notifications.ViewModels;

public class UnreadNotificationsViewModel
{
    public required Member CurrentMember { get; init; }

    public required PlatformType Platform { get; init; }

    public required int TotalCount { get; init; }

    public required IReadOnlyCollection<Notification> Unread { get; init; }
}