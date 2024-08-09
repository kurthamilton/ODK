using ODK.Core;
using ODK.Core.Events;
using ODK.Core.Extensions;
using ODK.Core.Members;
using ODK.Core.Utils;
using ODK.Core.Venues;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Authorization;
using ODK.Services.Events.ViewModels;

namespace ODK.Services.Events;

public class EventViewModelService : IEventViewModelService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUnitOfWork _unitOfWork;

    public EventViewModelService(
        IUnitOfWork unitOfWork, 
        IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<EventPageViewModel> GetEventPageViewModel(Guid? currentMemberId, string chapterName, 
        Guid eventId)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetByName(chapterName).RunAsync();
        OdkAssertions.Exists(chapter);

        var (
            membershipSettings, 
            @event, 
            venue, 
            member, 
            memberSubscription, 
            hosts, 
            comments,
            responses) = await _unitOfWork.RunAsync(
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.EventRepository.GetById(eventId),
            x => x.VenueRepository.GetByEventId(eventId),
            x => currentMemberId != null 
                ? x.MemberRepository.GetByIdOrDefault(currentMemberId.Value) 
                : new DefaultDeferredQuerySingleOrDefault<Member>(),
            x => currentMemberId != null
                ? x.MemberSubscriptionRepository.GetByMemberIdOrDefault(currentMemberId.Value, chapter.Id)
                : new DefaultDeferredQuerySingleOrDefault<MemberSubscription>(),
            x => x.EventHostRepository.GetByEventId(eventId),
            x => x.EventCommentRepository.GetByEventId(eventId),
            x => x.EventResponseRepository.GetByEventId(eventId));

        OdkAssertions.BelongsToChapter(@event, chapter.Id);

        var isActive = memberSubscription != null 
            ? _authorizationService.MembershipIsActive(memberSubscription, membershipSettings)
            : false;

        IReadOnlyCollection<Member> commentMembers = [];
        IReadOnlyCollection<Member> responseMembers = [];

        var canRespond = @event.IsAuthorized(member) && isActive;
        if (!canRespond)
        {
            comments = [];
            responses = [];
        }
        else
        {
            var commentMemberIds = comments
                .Select(x => x.MemberId)
                .Distinct()
                .ToArray();

            var responseMemberIds = responses
                .Select(x => x.MemberId)
                .Distinct()
                .ToArray();

            var memberIds = commentMemberIds
                .Concat(responseMemberIds)
                .Distinct()
                .ToArray();

            if (memberIds.Any())
            {
                var members = await _unitOfWork.MemberRepository
                    .GetByChapterId(chapter.Id, memberIds)
                    .RunAsync();

                var memberDictionary = members.ToDictionary(x => x.Id);

                commentMembers = commentMemberIds
                    .Where(memberDictionary.ContainsKey)
                    .Select(x => memberDictionary[x])
                    .ToArray();

                responseMembers = responseMemberIds
                    .Where(memberDictionary.ContainsKey)
                    .Select(x => memberDictionary[x])
                    .ToArray();
            }
        }

        var responseMemberDictionary = responseMembers.ToDictionary(x => x.Id);

        var responseDictionary = responses
            .Where(x => responseMemberDictionary.ContainsKey(x.MemberId))
            .GroupBy(x => x.Type)
            .ToDictionary(
                x => x.Key, 
                x => (IReadOnlyCollection<Member>)x
                    .Select(response => responseMemberDictionary[response.MemberId])
                    .ToArray());

        return new EventPageViewModel
        {
            CanRespond = canRespond,
            Chapter = chapter,
            Comments = new EventCommentsDto
            {
                Comments = comments,
                Members = commentMembers
            },
            CurrentMember = member,
            Event = @event,
            Hosts = hosts.Select(x => x.Member).ToArray(),            
            MembersByResponse = responseDictionary,
            MemberResponse = currentMemberId != null 
                ? responses.FirstOrDefault(x => x.MemberId == currentMemberId.Value)?.Type
                : null,
            Venue = venue
        };
    }

    public async Task<EventsPageViewModel> GetEventsPage(Guid? currentMemberId, string chapterName)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetByName(chapterName).RunAsync();
        OdkAssertions.Exists(chapter);

        var currentTime = chapter.CurrentTime();
        var afterUtc = currentTime.StartOfDay();

        var (member, events) = await _unitOfWork.RunAsync(
            x => currentMemberId != null ? x.MemberRepository.GetByIdOrDefault(currentMemberId.Value) : new DefaultDeferredQuerySingleOrDefault<Member>(),
            x => x.EventRepository.GetByChapterId(chapter.Id, afterUtc));

        events = events
            .Where(x => member?.IsMemberOf(chapter.Id) == true || x.IsPublic)
            .ToArray();

        var eventIds = events.Select(x => x.Id).ToArray();
        var venueIds = events.Select(x => x.VenueId).Distinct().ToArray();

        IReadOnlyCollection<EventResponse> responses = [];
        IReadOnlyCollection<EventInvite> invites = [];
        IReadOnlyCollection<Guid> invitedEventIds = [];
        IReadOnlyCollection<Venue> venues = [];

        if (eventIds.Any() && member?.IsMemberOf(chapter.Id) == true)
        {
            (venues, responses, invites) = await _unitOfWork.RunAsync(
                x => x.VenueRepository.GetByEventIds(eventIds),
                x => x.EventResponseRepository.GetByMemberId(member.Id, eventIds),
                x => x.EventInviteRepository.GetByMemberId(member.Id, eventIds));

            invitedEventIds = invites
                .Select(x => x.EventId)
                .Distinct()
                .ToArray();
        }
        else if (eventIds.Any())
        {
            venues = await _unitOfWork.VenueRepository.GetByEventIds(eventIds).RunAsync();
        }

        var responseLookup = responses
            .ToDictionary(x => x.EventId, x => x.Type);

        var venueLookup = venues
            .GroupBy(x => x.Id)
            .ToDictionary(x => x.Key, x => x.First());

        var viewModels = new List<EventResponseViewModel>();
        foreach (var @event in events.Where(x => x.PublishedUtc != null))
        {
            var invited = invitedEventIds.Contains(@event.Id);
            responseLookup.TryGetValue(@event.Id, out EventResponseType responseType);
            var viewModel = new EventResponseViewModel(@event, venueLookup[@event.VenueId],
                responseType, invited, @event.IsPublic);
            viewModels.Add(viewModel);
        }

        return new EventsPageViewModel
        {
            Chapter = chapter,
            CurrentMember = member,
            Events = viewModels
        };
    }
}
