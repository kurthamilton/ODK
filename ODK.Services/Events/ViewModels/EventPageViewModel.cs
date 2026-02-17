using ODK.Core.Events;
using ODK.Core.Venues;
using ODK.Data.Core.Members;
using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Events.ViewModels;

public class EventPageViewModel : GroupPageViewModel
{
    public required decimal AmountPaid { get; init; }

    public required decimal AmountRemaining { get; init; }

    public required bool CanRespond { get; init; }

    public bool CanRsvp =>
        MemberResponse == EventResponseType.Yes ||
        (
            (SpacesLeft == null || SpacesLeft > 0) &&
            !Event.RsvpDeadlinePassed &&
            !Event.RsvpDisabled
        );

    public bool CanShowWaitlist =>
        !CanRsvp &&
        !Event.RsvpDeadlinePassed &&
        !Event.WaitlistDisabled;

    public required bool CanView { get; init; }

    public required EventCommentsDto Comments { get; init; }

    public required Event Event { get; init; }

    public required IReadOnlyCollection<MemberWithAvatarDto> Hosts { get; init; }

    public required bool IsOnWaitlist { get; init; }

    public required EventResponseType? MemberResponse { get; init; }

    public required IReadOnlyDictionary<EventResponseType, IReadOnlyCollection<MemberWithAvatarDto>> MembersByResponse { get; init; }

    public IReadOnlyCollection<EventResponseType> ResponseTypes { get; } = [EventResponseType.Yes, EventResponseType.Maybe, EventResponseType.No];

    public required int? SpacesLeft { get; init; }

    public required Venue? Venue { get; init; }

    public required VenueLocation? VenueLocation { get; init; }

    public required int WaitlistLength { get; init; }
}