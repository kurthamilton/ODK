using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Venues;
using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Events.ViewModels;

public class EventPageViewModel : GroupPageViewModel
{
    public required decimal AmountPaid { get; init; }

    public required decimal AmountRemaining { get; init; }

    public required bool CanRespond { get; init; }

    public required bool CanView { get; init; }

    public required ChapterPaymentSettings? ChapterPaymentSettings { get; init; }

    public required EventCommentsDto Comments { get; init; }

    public required Event Event { get; init; }

    public required IReadOnlyCollection<Member> Hosts { get; init; }

    public required EventResponseType? MemberResponse { get; init; }

    public required IReadOnlyDictionary<EventResponseType, IReadOnlyCollection<Member>> MembersByResponse { get; init; }

    public IReadOnlyCollection<EventResponseType> ResponseTypes { get; } = [EventResponseType.Yes, EventResponseType.Maybe, EventResponseType.No];

    public int? SpacesLeft
    {
        get
        {
            var numberOfAttendees = MembersByResponse.TryGetValue(EventResponseType.Yes, out var attendees)
                ? attendees.Count
                : 0;

            return Event.NumberOfSpacesLeft(numberOfAttendees);
        }
    }

    public required Venue? Venue { get; init; }

    public required VenueLocation? VenueLocation { get; init; }
}