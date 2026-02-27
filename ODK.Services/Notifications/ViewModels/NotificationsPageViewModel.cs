using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Members;

namespace ODK.Services.Notifications.ViewModels;

public class NotificationsPageViewModel
{
    public required IReadOnlyCollection<ChapterAdminMemberDto> AdminChapters { get; init; }

    public required IReadOnlyCollection<MemberChapterNotificationSettings> ChapterSettings { get; init; }

    public required IReadOnlyCollection<MemberChapterDto> MemberChapters { get; init; }

    public required PlatformType Platform { get; init; }

    public required IReadOnlyCollection<MemberNotificationSettings> Settings { get; init; }
}