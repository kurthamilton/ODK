using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Venues;

namespace ODK.Services.Events.ViewModels;

public class EventPageViewModel
{
    public required bool CanRespond { get; init; }

    public required Chapter Chapter { get; init; }

    public required Member? CurrentMember { get; init; }

    public required EventCommentsDto Comments { get; init; }

    public required Event Event { get; init; }

    public required IReadOnlyCollection<Member> Hosts { get; init; }

    public required EventResponseType? MemberResponse { get; init; }

    public required IReadOnlyDictionary<EventResponseType, IReadOnlyCollection<Member>> MembersByResponse { get; init; }

    public required Venue Venue { get; init; }
}
