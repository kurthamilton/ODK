using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Topics;
using ODK.Data.Core.Chapters;
using ODK.Services.SocialMedia.ViewModels;

namespace ODK.Services.Chapters.ViewModels;

public class GroupHomePageViewModel : GroupPageViewModel
{
    public required ChapterLocation? ChapterLocation { get; init; }

    public required ChapterImageVersionDto? Image { get; init; }

    public required InstagramPostsViewModel InstagramPosts { get; init; }

    public required ChapterLinks? Links { get; init; }

    public required int MemberCount { get; init; }

    public required IReadOnlyCollection<Member> Owners { get; init; }

    public required IReadOnlyCollection<GroupPageListEventViewModel> RecentEvents { get; init; }

    public required ChapterTexts? Texts { get; init; }

    public required IReadOnlyCollection<Topic> Topics { get; init; }

    public required IReadOnlyCollection<GroupPageListEventViewModel> UpcomingEvents { get; init; }
}