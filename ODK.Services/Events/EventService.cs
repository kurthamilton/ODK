using ODK.Core.Events;
using ODK.Core.Exceptions;
using ODK.Core.Members;
using ODK.Data.Core;
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

    public async Task<Event> GetEvent(Guid chapterId, Guid eventId)
    {
        var @event = await _unitOfWork.EventRepository.GetById(eventId).RunAsync();
        if (@event.ChapterId != chapterId)
        {
            throw new OdkNotFoundException();
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
        Guid chapterId, DateTime? afterUtc)
    {
        var isChapterMember = member?.IsMemberOf(chapterId) == true;

        var (events, venues) = await _unitOfWork.RunAsync(
            x => isChapterMember ? x.EventRepository.GetByChapterId(chapterId, afterUtc) : x.EventRepository.GetPublicEventsByChapterId(chapterId, afterUtc),
            x => x.VenueRepository.GetByChapterId(chapterId));

        IReadOnlyCollection<EventResponse> responses = [];
        IReadOnlyCollection<EventInvite> invites = [];
        var invitedEventIds = new HashSet<Guid>();
        if (member != null)
        {
            (responses, invites) = await _unitOfWork.RunAsync(
                x => x.EventResponseRepository.GetByMemberId(member.Id, afterUtc),
                x => x.EventInviteRepository.GetByMemberId(member.Id));

            invitedEventIds = new HashSet<Guid>(invites.Select(x => x.EventId));
        }

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
    
    public async Task<ServiceResult> UpdateMemberResponse(Member member, Guid eventId,
        EventResponseType responseType)
    {
        responseType = NormalizeResponseType(responseType);

        var (@event, response) = await _unitOfWork.RunAsync(
            x => x.EventRepository.GetById(eventId),
            x => x.EventResponseRepository.GetByMemberId(member.Id, eventId));
        if (@event.DateUtc < DateTime.Today)
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
