using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Authorization;

namespace ODK.Services.Events;

public class EventService : IEventService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUnitOfWork _unitOfWork;

    public EventService(IUnitOfWork unitOfWork,       
        IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Event?> GetEvent(Guid chapterId, Guid eventId)
    {
        var @event = await _unitOfWork.EventRepository.GetById(eventId).RunAsync();
        if (@event == null || @event.ChapterId != chapterId)
        {
            return null;
        }

        return @event;
    }
    
    public async Task<EventResponsesDto> GetEventResponsesDto(Event @event)
    {
        var (responses, members) = await _unitOfWork.RunAsync(
            x => x.EventResponseRepository.GetByEventId(@event.Id),
            x => x.MemberRepository.GetByChapterId(@event.ChapterId));

        return new EventResponsesDto
        {
            Members = members,
            Responses = responses
        };
    }

    public async Task<IReadOnlyCollection<EventResponseViewModel>> GetEventResponseViewModels(Member? member, Guid chapterId)
    {
        return await GetEventResponseViewModels(member, chapterId, DateTime.Today);
    }

    public async Task<IReadOnlyCollection<EventResponseViewModel>> GetEventResponseViewModels(Member? member,
        Guid chapterId, DateTime? after)
    {
        var isChapterMember = member?.ChapterId == chapterId;

        var events = isChapterMember
            ? await _unitOfWork.EventRepository.GetByChapterId(chapterId, after).RunAsync()
            : await _unitOfWork.EventRepository.GetPublicEventsByChapterId(chapterId, after).RunAsync();

        var eventIds = events.Select(x => x.Id).ToArray();

        var venueIds = events
            .Select(x => x.VenueId)
            .Distinct()
            .ToArray();

        var (venues, responses, invites) = await _unitOfWork.RunAsync(
            x => x.VenueRepository.GetByChapterId(chapterId, venueIds),
            x => member != null 
                ? x.EventResponseRepository.GetByMemberId(member.Id, eventIds) 
                : DeferreredDefaults.Multiple<EventResponse>(),
            x => member != null 
                ? x.EventInviteRepository.GetByMemberId(member.Id, eventIds)
                : DeferreredDefaults.Multiple<EventInvite>());

        var invitedEventIds = new HashSet<Guid>(invites.Select(x => x.EventId));

        var responseLookup = responses
            .ToDictionary(x => x.EventId, x => x.ResponseTypeId);

        var venueLookup = venues.ToDictionary(x => x.Id);        

        var viewModels = new List<EventResponseViewModel>();
        foreach (Event @event in events)
        {
            var invited = invitedEventIds.Contains(@event.Id);
            responseLookup.TryGetValue(@event.Id, out EventResponseType responseType);
            var viewModel = new EventResponseViewModel(@event, venueLookup[@event.VenueId], 
                responseType, invited, @event.IsPublic);
            viewModels.Add(viewModel);
        }

        return viewModels;
    }
    
    public async Task<ServiceResult> UpdateMemberResponse(Guid memberId, Guid eventId, EventResponseType responseType)
    {
        responseType = NormalizeResponseType(responseType);

        var (member, @event, response) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(memberId),
            x => x.EventRepository.GetById(eventId),
            x => x.EventResponseRepository.GetByMemberId(memberId, eventId));
        if (@event.Date < DateTime.Today)
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

        if (response == null)
        {
            _unitOfWork.EventResponseRepository.Add(new EventResponse
            {
                EventId = eventId,
                MemberId = member.Id,
                ResponseTypeId = responseType
            });
        }
        else
        {
            response.ResponseTypeId = responseType;
            _unitOfWork.EventResponseRepository.Update(response);
        }

        await _unitOfWork.SaveChangesAsync();

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
