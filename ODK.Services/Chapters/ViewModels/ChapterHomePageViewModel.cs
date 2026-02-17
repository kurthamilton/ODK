using ODK.Core.Chapters;
using ODK.Core.Topics;
using ODK.Data.Core.Members;
using ODK.Services.Events.ViewModels;
using ODK.Services.SocialMedia.ViewModels;
using ODK.Services.ViewModels;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterHomePageViewModel : ChapterViewModelBase
{
    public required ChapterLocation? ChapterLocation { get; init; }

    public required IReadOnlyCollection<EventResponseViewModel> Events { get; init; }

    public required InstagramPostsViewModel InstagramPosts { get; init; }

    public required IReadOnlyCollection<MemberWithAvatarDto> LatestMembers { get; init; }

    public required ChapterLinks? Links { get; init; }

    public required ChapterTexts? Texts { get; init; }

    public required IReadOnlyCollection<Topic> Topics { get; init; }

    public required string? WhatsAppUrl { get; init; }
}