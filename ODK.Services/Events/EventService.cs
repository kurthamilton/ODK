using System.Text.RegularExpressions;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Chapters;
using ODK.Services.Emails;

namespace ODK.Services.Events;

public class EventService : IEventService
{
    private static readonly Regex HideCommentRegex = new Regex("http://|https://|<script>.*</script>|<img", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly IAuthorizationService _authorizationService;
    private readonly IChapterUrlService _chapterUrlService;
    private readonly IEmailService _emailService;
    private readonly EventServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public EventService(IUnitOfWork unitOfWork,       
        IAuthorizationService authorizationService,
        IEmailService emailService,
        EventServiceSettings settings,
        IChapterUrlService chapterUrlService)
    {
        _authorizationService = authorizationService;
        _chapterUrlService = chapterUrlService;
        _emailService = emailService;
        _settings = settings;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> AddComment(Guid currentMemberId, Guid eventId, string comment, Guid? parentEventCommentId)
    {
        var (member, @event) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.EventRepository.GetById(eventId));

        var (chapter, settings) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(@event.ChapterId),
            x => x.ChapterEventSettingsRepository.GetByChapterId(@event.ChapterId));        

        if (settings?.DisableComments == true || !@event.CanComment || !@event.IsAuthorized(member))
        {
            return ServiceResult.Failure("You cannot comment on this event");
        }        

        if (string.IsNullOrWhiteSpace(comment))
        {
            return ServiceResult.Failure("Comment required");
        }

        EventComment? parentComment = null;
        Member? parentCommentMember = null;
        if (parentEventCommentId != null)
        {
            parentComment = await _unitOfWork.EventCommentRepository.GetByIdOrDefault(parentEventCommentId.Value).RunAsync();
            if (parentComment != null && parentComment.EventId != @event.Id)
            {
                parentComment = null;
            }

            if (parentComment != null)
            {
                parentCommentMember = await _unitOfWork.MemberRepository.GetById(parentComment.MemberId).RunAsync();
            }
        }

        var hidden = HideCommentRegex.IsMatch(comment);

        var eventComment = new EventComment
        {
            CreatedUtc = DateTime.UtcNow,
            EventId = eventId,
            Hidden = hidden,
            MemberId = currentMemberId,
            ParentEventCommentId = parentComment?.Id,
            Text = comment
        };
        _unitOfWork.EventCommentRepository.Add(eventComment);
        await _unitOfWork.SaveChangesAsync();

        var parameters = new Dictionary<string, string>
        {
            { "comment.text", eventComment.Text },
            { "event.id", @event.Id.ToString() }
        };

        var url = _chapterUrlService.GetChapterUrl(chapter, _settings.EventUrlFormat, parameters);
        parameters.Add("event.url", url);

        await _emailService.SendEventCommentEmail(chapter, parentCommentMember, eventComment, parameters);

        return ServiceResult.Successful();
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
            .ToDictionary(x => x.EventId, x => x.Type);

        var venueLookup = venues.ToDictionary(x => x.Id);        

        var viewModels = new List<EventResponseViewModel>();
        foreach (var @event in events.Where(x => x.PublishedUtc != null))
        {
            var invited = invitedEventIds.Contains(@event.Id);
            responseLookup.TryGetValue(@event.Id, out EventResponseType responseType);
            var viewModel = new EventResponseViewModel(@event, venueLookup[@event.VenueId], 
                responseType, invited);
            viewModels.Add(viewModel);
        }

        return viewModels;
    }
    
    public async Task<ServiceResult> UpdateMemberResponse(Guid memberId, Guid eventId,
        EventResponseType responseType)
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
                Type = responseType
            });
        }
        else
        {
            response.Type = responseType;
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

    private async Task<EventCommentsDto> GetCommentsDto(Member? member, Event @event, ChapterEventSettings settings, IReadOnlyCollection<EventComment> comments)
    {        
        if (settings?.DisableComments == true || !@event.IsAuthorized(member) || !@event.CanComment)
        {
            return new EventCommentsDto
            {
                Comments = null,
                Members = null
            };
        }

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
}
