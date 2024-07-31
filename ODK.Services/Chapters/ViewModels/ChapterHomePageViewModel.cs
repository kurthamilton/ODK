using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.SocialMedia;
using ODK.Core.Venues;
using ODK.Services.ViewModels;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterHomePageViewModel : ChapterViewModelBase
{    
    public required IReadOnlyCollection<Event> Events { get; init; }

    public required IReadOnlyCollection<Venue> EventVenues { get; init; }

    public required IReadOnlyCollection<InstagramPost> InstagramPosts { get; init; }

    public required IReadOnlyCollection<Member> LatestMembers { get; init; }

    public required ChapterLinks? Links { get; init; }

    public required IReadOnlyCollection<EventResponse> MemberEventResponses { get; init; }

    public required ChapterTexts? Texts { get; init; }    
}
