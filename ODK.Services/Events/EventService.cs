using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Exceptions;
using ODK.Core.Members;
using ODK.Core.Utils;
using ODK.Data.Core;
using ODK.Services.Authorization;

namespace ODK.Services.Events;

public class EventService : IEventService
{
    private static readonly Regex HideCommentRegex = new Regex("http://|https://|<script>.*</script>|<img", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly IAuthorizationService _authorizationService;
    private readonly IUnitOfWork _unitOfWork;

    public EventService(IUnitOfWork unitOfWork,       
        IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> AddComment(Guid currentMemberId, Guid eventId, string comment)
    {
        var (member, @event) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.EventRepository.GetById(eventId));

        var settings = await _unitOfWork.ChapterEventSettingsRepository.GetByChapterId(@event.ChapterId).RunAsync();
        if (settings?.DisableComments == true || !@event.CanComment || !@event.IsAuthorized(member))
        {
            return ServiceResult.Failure("You cannot comment on this event");
        }        

        if (string.IsNullOrWhiteSpace(comment))
        {
            return ServiceResult.Failure("Comment required");
        }

        var hidden = HideCommentRegex.IsMatch(comment);

        _unitOfWork.EventCommentRepository.Add(new EventComment
        {
            CreatedUtc = DateTime.UtcNow,
            EventId = eventId,
            Hidden = hidden,
            MemberId = currentMemberId,
            Text = comment
        });
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<EventCommentsDto> GetCommentsDto(Member? member, Event @event)
    {
        var settings = await _unitOfWork.ChapterEventSettingsRepository.GetByChapterId(@event.ChapterId).RunAsync();
        if (settings?.DisableComments == true || !@event.IsAuthorized(member) || !@event.CanComment)
        {
            return new EventCommentsDto
            {
                Comments = null,
                Members = null
            };
        }

        var comments = await _unitOfWork.EventCommentRepository.GetByEventId(@event.Id).RunAsync();
        var memberIds = comments
            .Select(x => x.MemberId)
            .Distinct()
            .ToArray();

        var members = memberIds.Length > 0
            ? await _unitOfWork.MemberRepository.GetByChapterId(@event.ChapterId, memberIds).RunAsync()
            : [];

        return new EventCommentsDto
        {
            Comments = comments,
            Members = members
        };
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

    public async Task<IReadOnlyCollection<EventResponseViewModel>> GetEventResponseViewModels(Member? member, Chapter chapter)
    {
        var currentTime = chapter.CurrentTime;
        var after = currentTime.StartOfDay();

        return await GetEventResponseViewModels(member, chapter.Id, after);
    }

    public async Task<IReadOnlyCollection<EventResponseViewModel>> GetEventResponseViewModels(Member? member,
        Guid chapterId, DateTime? afterUtc)
    {
        var isChapterMember = member?.IsMemberOf(chapterId) == true;

        var (events, venues) = await _unitOfWork.RunAsync(
            x => isChapterMember 
                ? x.EventRepository.GetByChapterId(chapterId, afterUtc) 
                : x.EventRepository.GetPublicEventsByChapterId(chapterId, afterUtc),
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
