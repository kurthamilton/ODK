using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Extensions;
using ODK.Core.Members;
using ODK.Core.Notifications;
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

    public async Task<EventPageViewModel> GetEventPageViewModel(
        ServiceRequest request, Guid? currentMemberId, string chapterName, Guid eventId)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetByName(chapterName).Run();
        OdkAssertions.Exists(chapter, $"Chapter not found: '{chapterName}'");

        return await GetEventPageViewModel(request, currentMemberId, chapter, eventId);
    }

    public async Task<EventsPageViewModel> GetEventsPage(
        ServiceRequest request, Guid? currentMemberId, string chapterName)
    {
        var platform = request.Platform;

        var chapter = await _unitOfWork.ChapterRepository.GetByName(chapterName).Run();
        OdkAssertions.Exists(chapter, $"Chapter not found: '{chapterName}'");

        var currentTime = chapter.CurrentTime();
        var afterUtc = currentTime.StartOfDay();

        var (chapterPrivacySettings, membershipSettings, member, memberSubscription, events) = await _unitOfWork.RunAsync(
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => currentMemberId != null 
                ? x.MemberRepository.GetByIdOrDefault(currentMemberId.Value) 
                : new DefaultDeferredQuerySingleOrDefault<Member>(),
            x => currentMemberId != null 
                ? x.MemberSubscriptionRepository.GetByMemberId(currentMemberId.Value, chapter.Id) 
                : new DefaultDeferredQuerySingleOrDefault<MemberSubscription>(),
            x => x.EventRepository.GetByChapterId(chapter.Id, afterUtc));

        var eventIds = events.Select(x => x.Id).ToArray();
        var venueIds = events.Select(x => x.VenueId).Distinct().ToArray();

        IReadOnlyCollection<EventResponse> memberResponses = [];
        IReadOnlyCollection<EventInvite> invites = [];
        IReadOnlyCollection<Guid> invitedEventIds = [];
        IReadOnlyCollection<Venue> venues = [];
        IReadOnlyCollection<EventResponseSummaryDto> responseSummaries = [];

        if (eventIds.Length > 0 && member?.IsMemberOf(chapter.Id) == true)
        {
            (venues, memberResponses, invites, responseSummaries) = await _unitOfWork.RunAsync(
                x => x.VenueRepository.GetByEventIds(eventIds),
                x => x.EventResponseRepository.GetByMemberId(member.Id, eventIds),
                x => x.EventInviteRepository.GetByMemberId(member.Id, eventIds),
                x => x.EventResponseRepository.GetResponseSummaries(eventIds));

            invitedEventIds = invites
                .Select(x => x.EventId)
                .Distinct()
                .ToArray();
        }
        else if (eventIds.Length > 0)
        {
            (venues, responseSummaries) = await _unitOfWork.RunAsync(
                x => x.VenueRepository.GetByEventIds(eventIds),
                x => x.EventResponseRepository.GetResponseSummaries(eventIds));
        }

        var memberResponseLookup = memberResponses
            .ToDictionary(x => x.EventId, x => x.Type);

        var venueLookup = venues
            .GroupBy(x => x.Id)
            .ToDictionary(x => x.Key, x => x.First());

        var responseSummaryLookup = responseSummaries
            .ToDictionary(x => x.EventId);

        var viewModels = new List<EventResponseViewModel>();
        foreach (var @event in events.Where(x => x.PublishedUtc != null))
        {
            var canViewEvent = _authorizationService.CanViewEvent(@event, member, memberSubscription, membershipSettings, chapterPrivacySettings);
            if (!canViewEvent)
            {
                continue;
            }

            var venue = venueLookup[@event.VenueId];
            var canViewVenue = _authorizationService.CanViewVenue(venue, member, memberSubscription, membershipSettings, chapterPrivacySettings);

            var invited = invitedEventIds.Contains(@event.Id);
            memberResponseLookup.TryGetValue(@event.Id, out EventResponseType responseType);

            responseSummaryLookup.TryGetValue(@event.Id, out var responseSummary);

            var viewModel = new EventResponseViewModel(
                @event: @event, 
                venue: canViewVenue ? venue : null, 
                response: responseType, 
                invited: invited,
                responseSummary: responseSummary);
            viewModels.Add(viewModel);
        }

        return new EventsPageViewModel
        {
            Chapter = chapter,
            CurrentMember = member,
            Events = viewModels.OrderBy(x => x.Date).ToArray(),
            Platform = platform
        };
    }

    public async Task<EventPageViewModel> GetGroupEventPageViewModel(
        ServiceRequest request, Guid? currentMemberId, string slug, Guid eventId)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).Run();
        OdkAssertions.Exists(chapter, $"Chapter not found: '{slug}'");

        return await GetEventPageViewModel(request, currentMemberId, chapter, eventId);
    }

    private async Task<EventPageViewModel> GetEventPageViewModel(
        ServiceRequest request, Guid? currentMemberId, Chapter chapter, Guid eventId)
    {
        var platform = request.Platform;

        var (
            membershipSettings,
            @event,
            venue,
            member,
            memberSubscription,
            hosts,
            comments,
            responses,
            ticketPurchases,
            chapterPaymentSettings,
            privacySettings,
            hasQuestions,
            adminMembers,
            ownerSubscription,
            notifications) = await _unitOfWork.RunAsync(
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.EventRepository.GetById(eventId),
            x => x.VenueRepository.GetByEventId(eventId),
            x => currentMemberId != null
                ? x.MemberRepository.GetByIdOrDefault(currentMemberId.Value)
                : new DefaultDeferredQuerySingleOrDefault<Member>(),
            x => currentMemberId != null
                ? x.MemberSubscriptionRepository.GetByMemberId(currentMemberId.Value, chapter.Id)
                : new DefaultDeferredQuerySingleOrDefault<MemberSubscription>(),
            x => x.EventHostRepository.GetByEventId(eventId),
            x => x.EventCommentRepository.GetByEventId(eventId),
            x => x.EventResponseRepository.GetByEventId(eventId),
            x => x.EventTicketPurchaseRepository.GetByEventId(eventId),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => currentMemberId != null 
                ? x.NotificationRepository.GetUnreadByMemberId(currentMemberId.Value, NotificationType.NewEvent, eventId)
                : new DefaultDeferredQueryMultiple<Notification>());

        OdkAssertions.BelongsToChapter(@event, chapter.Id);

        var canViewEvent = _authorizationService.CanViewEvent(@event, member, memberSubscription, membershipSettings, privacySettings);
        var canViewVenue = _authorizationService.CanViewVenue(venue, member, memberSubscription, membershipSettings, privacySettings);
        var canRespond = _authorizationService.CanRespondToEvent(@event, member, memberSubscription, membershipSettings, privacySettings);

        IReadOnlyCollection<Member> commentMembers = [];
        IReadOnlyCollection<Member> responseMembers = [];

        if (!canViewEvent)
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

            if (memberIds.Length > 0)
            {
                var members = await _unitOfWork.MemberRepository
                    .GetByChapterId(chapter.Id, memberIds)
                    .Run();

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

        var venueLocation = canViewVenue
            ? await _unitOfWork.VenueLocationRepository.GetByVenueId(venue.Id)
            : null;

        if (notifications.Count > 0)
        {
            _unitOfWork.NotificationRepository.MarkAsRead(notifications);
            await _unitOfWork.SaveChangesAsync();
        }

        return new EventPageViewModel
        {
            CanRespond = canRespond,
            CanView = canViewEvent,
            Chapter = chapter,
            ChapterPaymentSettings = chapterPaymentSettings,
            Comments = new EventCommentsDto
            {
                Comments = comments,
                Members = commentMembers
            },
            CurrentMember = member,
            Event = @event,
            HasQuestions = hasQuestions,
            Hosts = hosts.Select(x => x.Member).ToArray(),
            IsAdmin = adminMembers.Any(x => x.MemberId == currentMemberId),
            IsMember = member?.IsMemberOf(chapter.Id) == true,
            MembersByResponse = responseDictionary,
            MemberResponse = currentMemberId != null
                ? responses.FirstOrDefault(x => x.MemberId == currentMemberId.Value)?.Type
                : null,
            OwnerSubscription = ownerSubscription,
            Platform = platform,
            TicketPurchases = ticketPurchases,
            Venue = canViewVenue ? venue : null,
            VenueLocation = venueLocation
        };
    }
}
