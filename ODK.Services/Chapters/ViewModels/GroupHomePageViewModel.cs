using ODK.Core.Chapters;
using ODK.Core.Topics;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Members;
using ODK.Services.SocialMedia.ViewModels;

namespace ODK.Services.Chapters.ViewModels;

public class GroupHomePageViewModel : GroupPageViewModel
{
    public required ChapterLocation? ChapterLocation { get; init; }

    public required ChapterImageVersionDto? Image { get; init; }

    public required InstagramPostsViewModel InstagramPosts { get; init; }

    public required ChapterLinks? Links { get; init; }

    public required int MemberCount { get; init; }

    public required IReadOnlyCollection<MemberWithAvatarDto> Owners { get; init; }

    public required IReadOnlyCollection<GroupPageListEventViewModel> RecentEvents { get; init; }

    /// <summary>
    /// The group's move here, while it is recent enough to be worth announcing; null once the window has
    /// passed, or where the group never declared one. Non-null is the whole condition for the welcome
    /// banner, so a view renders it without repeating the date arithmetic.
    /// </summary>
    public required ChapterMigration? RecentMove { get; init; }

    public required ChapterTexts? Texts { get; init; }

    public required IReadOnlyCollection<Topic> Topics { get; init; }

    public required IReadOnlyCollection<GroupPageListEventViewModel> UpcomingEvents { get; init; }
}