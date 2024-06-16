using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Venues;
using ODK.Services.Authorization;

namespace ODK.Services.Events;

public class EventService : IEventService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IEventRepository _eventRepository;
    private readonly IVenueRepository _venueRepository;

    public EventService(IEventRepository eventRepository,
        IVenueRepository venueRepository,
        IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        _eventRepository = eventRepository;
        _venueRepository = venueRepository;
    }

    public async Task<Event?> GetEvent(Guid chapterId, Guid eventId)
    {
        Event? @event = await _eventRepository.GetEventAsync(eventId);
        if (@event == null || @event.ChapterId != chapterId)
        {
            return null;
        }

        return @event;
    }
    
    public async Task<IReadOnlyCollection<EventResponse>> GetEventResponses(Guid eventId)
    {
        return await _eventRepository.GetEventResponsesAsync(eventId);
    }

    public async Task<IReadOnlyCollection<EventResponseViewModel>> GetEventResponseViewModels(Member? member, Guid chapterId)
    {
        return await GetEventResponseViewModels(member, chapterId, DateTime.Today);
    }

    public async Task<IReadOnlyCollection<EventResponseViewModel>> GetEventResponseViewModels(Member? member,
        Guid chapterId, DateTime? after)
    {
        IReadOnlyCollection<Event> events;
        if (member == null || member.ChapterId != chapterId)
        {
            events = await _eventRepository.GetPublicEventsAsync(chapterId, after);
        }
        else
        {
            events = await _eventRepository.GetEventsAsync(chapterId, after);
        }

        IReadOnlyCollection<EventResponse> responses = [];
        if (member != null)
        {
            bool allEvents = after == null;
            responses = await _eventRepository.GetMemberResponsesAsync(member.Id, allEvents);
        }

        var responseLookup = responses
            .ToDictionary(x => x.EventId, x => x.ResponseTypeId);

        IReadOnlyCollection<Venue> venues = await _venueRepository.GetVenuesAsync(chapterId);
        IDictionary<Guid, Venue> venueLookup = venues.ToDictionary(x => x.Id);

        var invitedEventIds = new HashSet<Guid>();
        if (member != null)
        {
            IReadOnlyCollection<EventInvite> eventInvites = await _eventRepository
                .GetEventInvitesForMemberIdAsync(member.Id);
            invitedEventIds = new HashSet<Guid>(eventInvites.Select(x => x.EventId));
        }

        var viewModels = new List<EventResponseViewModel>();
        foreach (Event @event in events)
        {
            bool invited = invitedEventIds.Contains(@event.Id);
            responseLookup.TryGetValue(@event.Id, out EventResponseType responseType);
            EventResponseViewModel viewModel = new(@event, venueLookup[@event.VenueId], 
                responseType, invited, @event.IsPublic);
            viewModels.Add(viewModel);
        }

        return viewModels;
    }
    
    public async Task<ServiceResult> UpdateMemberResponse(Member member, Guid eventId,
        EventResponseType responseType)
    {
        responseType = NormalizeResponseType(responseType);

        Event? @event = await _eventRepository.GetEventAsync(eventId);
        if (@event == null || @event.Date < DateTime.Today)
        {
            return ServiceResult.Failure("Past events cannot be responded to");
        }

        if (!@event.IsAuthorized(member))
        {
            return ServiceResult.Failure("You are not permitted to respond to this event");
        }

        var active = await _authorizationService.MembershipIsActiveAsync(member.Id, @event.ChapterId);
        if (!active)
        {
            return ServiceResult.Failure("You are not permitted to respond to this event");
        }

        await _eventRepository.UpdateEventResponseAsync(new EventResponse(eventId, member.Id, responseType));

        return ServiceResult.Successful();
    }

    private static EventResponseType NormalizeResponseType(EventResponseType responseType)
    {
        if (!Enum.IsDefined(typeof(EventResponseType), responseType) || responseType <= EventResponseType.None)
        {
            responseType = EventResponseType.No;
        }

        return responseType;
    }
}
