using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Extensions;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Payments;
using ODK.Core.Subscriptions;
using ODK.Core.Utils;
using ODK.Core.Venues;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Events;
using ODK.Services.Authorization;
using ODK.Services.Events.ViewModels;
using ODK.Services.Exceptions;
using ODK.Services.Payments;
using ODK.Services.Payments.Models;

namespace ODK.Services.Events;

public class EventViewModelService : IEventViewModelService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly IUnitOfWork _unitOfWork;

    public EventViewModelService(
        IUnitOfWork unitOfWork,
        IAuthorizationService authorizationService,
        IPaymentProviderFactory paymentProviderFactory)
    {
        _authorizationService = authorizationService;
        _paymentProviderFactory = paymentProviderFactory;
        _unitOfWork = unitOfWork;
    }

    public async Task<EventCheckoutPageViewModel> GetEventCheckoutPageViewModel
        (ServiceRequest request, Guid currentMemberId, Chapter chapter, Guid eventId, string returnPath)
    {
        var platform = request.Platform;

        var (
            chapterPages,
            @event,
            venue,
            currentMember,
            memberSubscription,
            chapterPaymentSettings,
            chapterPaymentAccount,
            sitePaymentSettings,
            hasProfiles,
            hasQuestions,
            adminMembers,
            ownerSubscription,
            eventTicketPayments,
            membershipSettings,
            privacySettings) = await _unitOfWork.RunAsync(
            x => x.ChapterPageRepository.GetByChapterId(chapter.Id),
            x => x.EventRepository.GetById(eventId),
            x => x.VenueRepository.GetByEventId(eventId),
            x => x.MemberRepository.GetByIdOrDefault(currentMemberId),
            x => x.MemberSubscriptionRepository.GetByMemberId(currentMemberId, chapter.Id),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPaymentAccountRepository.GetByChapterId(chapter.Id),
            x => x.SitePaymentSettingsRepository.GetActive(),
            x => x.ChapterPropertyRepository.ChapterHasProperties(chapter.Id),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.EventTicketPaymentRepository.GetConfirmedPayments(currentMemberId, eventId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapter.Id));

        OdkAssertions.BelongsToChapter(@event, chapter.Id);

        if (@event.TicketSettings == null)
        {
            throw new OdkServiceException("Event is not ticketed");
        }

        var paymentProvider = _paymentProviderFactory.GetPaymentProvider(
            sitePaymentSettings, chapterPaymentAccount);

        var externalProductId = chapterPaymentSettings.ExternalProductId;
        if (string.IsNullOrEmpty(externalProductId))
        {
            externalProductId = await paymentProvider.CreateProduct(chapter.FullName);

            if (string.IsNullOrEmpty(externalProductId))
            {
                throw new OdkServiceException("Error starting event checkout");
            }

            chapterPaymentSettings.ExternalProductId = externalProductId;
            _unitOfWork.ChapterPaymentSettingsRepository.Update(chapterPaymentSettings);
            await _unitOfWork.SaveChangesAsync();
        }

        var paidSoFar = eventTicketPayments.Sum(x => x.Payment.Amount);
        var amountDue = paidSoFar == 0
            ? @event.TicketSettings.Deposit ?? @event.TicketSettings.Cost
            : @event.TicketSettings.Cost - paidSoFar;

        PaymentReasonType reason;

        if (@event.TicketSettings.Deposit == null)
        {
            reason = paidSoFar == 0
                ? PaymentReasonType.EventTicket
                : PaymentReasonType.EventTicketRemainder;
        }
        else
        {
            reason = paidSoFar == 0
                ? PaymentReasonType.EventTicketDeposit
                : PaymentReasonType.EventTicketRemainder;
        }

        var utcNow = DateTime.UtcNow;
        var paymentCheckoutSessionId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();

        var eventTicketPayment = new EventTicketPayment
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            PaymentId = paymentId
        };

        var metadata = new PaymentMetadataModel(
            reason,
            currentMember,
            eventTicketPayment,
            paymentCheckoutSessionId);

        var externalCheckoutSession = await paymentProvider.StartCheckout(
            request,
            currentMember.EmailAddress,
            new ExternalSubscriptionPlan
            {
                Amount = amountDue,
                CurrencyCode = @event.TicketSettings.Currency.Code,
                ExternalId = string.Empty,
                ExternalProductId = externalProductId,
                Frequency = SiteSubscriptionFrequency.None,
                Name = @event.GetDisplayName(),
                NumberOfMonths = 0,
                Recurring = false
            },
            returnPath,
            metadata);

        _unitOfWork.PaymentCheckoutSessionRepository.Add(new PaymentCheckoutSession
        {
            Id = paymentCheckoutSessionId,
            MemberId = currentMember.Id,
            PaymentId = paymentId,
            SessionId = externalCheckoutSession.SessionId,
            StartedUtc = utcNow
        });

        _unitOfWork.PaymentRepository.Add(new Payment
        {
            Amount = amountDue,
            ChapterId = chapter.Id,
            CreatedUtc = utcNow,
            CurrencyId = @event.TicketSettings.CurrencyId,
            ExternalId = externalCheckoutSession.PaymentId,
            Id = paymentId,
            MemberId = currentMember.Id,
            Reference = @event.GetDisplayName(),
            SitePaymentSettingId = null
        });

        _unitOfWork.EventTicketPaymentRepository.Add(eventTicketPayment);

        await _unitOfWork.SaveChangesAsync();

        var canViewVenue = _authorizationService.CanViewVenue(
            venue, currentMember, memberSubscription, membershipSettings, privacySettings);

        return new EventCheckoutPageViewModel
        {
            Chapter = chapter,
            ChapterPages = chapterPages,
            ClientSecret = externalCheckoutSession.ClientSecret,
            CurrentMember = currentMember,
            Event = @event,
            HasProfiles = hasProfiles,
            HasQuestions = hasQuestions,
            IsAdmin = adminMembers.Any(x => x.MemberId == currentMemberId),
            IsMember = currentMember.IsMemberOf(chapter.Id),
            OwnerSubscription = ownerSubscription,
            PaymentSettings = sitePaymentSettings,
            Platform = platform,
            Venue = canViewVenue ? venue : null,
        };
    }

    public async Task<EventPageViewModel> GetEventPageViewModel(
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
            privacySettings,
            hasProperties,
            hasQuestions,
            adminMembers,
            ownerSubscription,
            notifications,
            chapterPages,
            payments,
            waitingList) = await _unitOfWork.RunAsync(
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
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPropertyRepository.ChapterHasProperties(chapter.Id),
            x => x.ChapterQuestionRepository.ChapterHasQuestions(chapter.Id),
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => currentMemberId != null
                ? x.NotificationRepository.GetUnreadByMemberId(currentMemberId.Value, NotificationType.NewEvent, eventId)
                : new DefaultDeferredQueryMultiple<Notification>(),
            x => x.ChapterPageRepository.GetByChapterId(chapter.Id),
            x => currentMemberId != null
                ? x.EventTicketPaymentRepository.GetConfirmedPayments(currentMemberId.Value, eventId)
                : new DefaultDeferredQueryMultiple<EventTicketPayment>(),
            x => x.EventWaitingListMemberRepository.GetByEventId(eventId));

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

        var amountPaid = payments.Sum(x => x.Payment.Amount);

        return new EventPageViewModel
        {
            AmountPaid = amountPaid,
            AmountRemaining = @event.TicketSettings != null
                ? @event.TicketSettings.Cost - amountPaid
                : 0,
            CanRespond = canRespond,
            CanView = canViewEvent,
            Chapter = chapter,
            ChapterPages = chapterPages,
            Comments = new EventCommentsDto
            {
                Comments = comments,
                Members = commentMembers
            },
            CurrentMember = member,
            Event = @event,
            HasProfiles = hasProperties,
            HasQuestions = hasQuestions,
            Hosts = hosts.Select(x => x.Member).ToArray(),
            IsAdmin = adminMembers.Any(x => x.MemberId == currentMemberId),
            IsOnWaitingList = waitingList.Any(x => x.MemberId == currentMemberId),
            IsMember = member?.IsMemberOf(chapter.Id) == true,
            MembersByResponse = responseDictionary,
            MemberResponse = currentMemberId != null
                ? responses.FirstOrDefault(x => x.MemberId == currentMemberId.Value)?.Type
                : null,
            OwnerSubscription = ownerSubscription,
            SpacesLeft = responseDictionary.TryGetValue(EventResponseType.Yes, out var attendees)
                ? @event.NumberOfSpacesLeft(attendees.Count)
                : @event.NumberOfSpacesLeft(0),
            Platform = platform,
            Venue = canViewVenue ? venue : null,
            VenueLocation = venueLocation,
            WaitingListLength = waitingList.Count
        };
    }

    public async Task<EventsPageViewModel> GetEventsPage(
        ServiceRequest request, Guid? currentMemberId, Chapter chapter)
    {
        var platform = request.Platform;

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
}