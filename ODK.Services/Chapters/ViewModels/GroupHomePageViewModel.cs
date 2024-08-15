using ODK.Core.Chapters;
using ODK.Core.SocialMedia;

namespace ODK.Services.Chapters.ViewModels;

public class GroupHomePageViewModel : GroupPageViewModelBase
{
    public required ChapterLocation? ChapterLocation { get; init; }

    public required IReadOnlyCollection<InstagramPost> InstagramPosts { get; init; }

    public required ChapterLinks? Links { get; init; }

    public required int MemberCount { get; init; }

    public required IReadOnlyCollection<ChapterQuestion> Questions { get; init; }

    public required IReadOnlyCollection<GroupHomePageEventViewModel> RecentEvents { get; init; }

    public required IReadOnlyCollection<GroupHomePageEventViewModel> UpcomingEvents { get; init; }
}
