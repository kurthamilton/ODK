using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Extensions;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Utils;
using ODK.Core.Venues;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Events.ViewModels;
using ODK.Services.Exceptions;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.Notifications;
using ODK.Services.Payments;
using ODK.Services.Tasks;

namespace ODK.Services.Events;

public class EventAdminService : OdkAdminServiceBase, IEventAdminService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IBackgroundTaskService _backgroundTaskService;
    private readonly IEventService _eventService;
    private readonly IHtmlSanitizer _htmlSanitizer;
    private readonly ILoggingService _loggingService;
    private readonly IMemberEmailService _memberEmailService;
    private readonly INotificationService _notificationService;
    private readonly IPaymentService _paymentService;
    private readonly EventAdminServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public EventAdminService(
        IUnitOfWork unitOfWork,
        IAuthorizationService authorizationService,
        INotificationService notificationService,
        IHtmlSanitizer htmlSanitizer,
        IMemberEmailService memberEmailService,
        IBackgroundTaskService backgroundTaskService,
        ILoggingService loggingService,
        IPaymentService paymentService,
        IEventService eventService,
        EventAdminServiceSettings settings)
        : base(unitOfWork)
    {
        _authorizationService = authorizationService;
        _backgroundTaskService = backgroundTaskService;
        _eventService = eventService;
        _htmlSanitizer = htmlSanitizer;
        _loggingService = loggingService;
        _memberEmailService = memberEmailService;
        _notificationService = notificationService;
        _paymentService = paymentService;
        _settings = settings;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> CreateEvent(
        MemberChapterAdminServiceRequest request, CreateEvent model, bool draft)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        var (
            ownerSubscription,
            chapterAdminMembers,
            venue,
            settings,
            members,
            notificationSettings,
            chapterTopics,
            currency
        ) = await _unitOfWork.RunAsync(
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.VenueRepository.GetById(model.VenueId),
            x => x.ChapterEventSettingsRepository.GetByChapterId(chapter.Id),
            x => x.MemberRepository.GetAllByChapterId(chapter.Id),
            x => x.MemberNotificationSettingsRepository.GetByChapterId(chapter.Id, NotificationType.NewEvent),
            x => x.ChapterTopicRepository.GetByChapterId(chapter.Id),
            x => x.CurrencyRepository.GetByChapterId(chapter.Id));

        AssertMemberIsChapterAdmin(
            request,
            chapterAdminMembers.FirstOrDefault(x => x.MemberId == currentMember.Id));

        var date = Event.FromLocalTime(model.Date, chapter.TimeZone);
        var @event = new Event
        {
            AttendeeLimit = model.AttendeeLimit,
            ChapterId = chapter.Id,
            CreatedBy = currentMember.FullName,
            CreatedUtc = DateTime.UtcNow,
            Date = date,
            Description = model.Description != null
                ? _htmlSanitizer.Sanitize(model.Description, DefaultHtmlSantizerOptions)
                : null,
            EndTime = model.EndTime,
            ImageUrl = model.ImageUrl,
            IsPublic = model.IsPublic,
            Name = model.Name,
            PublishedUtc = !draft ? DateTime.UtcNow : null,
            RsvpDeadlineUtc = model.RsvpDeadline != null ? chapter.FromLocalTime(model.RsvpDeadline.Value) : null,
            RsvpDisabled = model.RsvpDisabled,
            Time = model.Time,
            VenueId = model.VenueId
        };

        if (ownerSubscription?.HasFeature(SiteFeatureType.EventTickets) == true)
        {
            @event.TicketSettings = model.TicketCost != null ? new EventTicketSettings
            {
                Cost = Math.Round(model.TicketCost.Value, 2),
                CurrencyId = currency.Id,
                Deposit = model.TicketDepositCost != null ? Math.Round(model.TicketDepositCost.Value, 2) : null,
            } : null;

            @event.WaitlistDisabled = @event.Ticketed;
        }

        var validationResult = ValidateEvent(@event, venue);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.EventRepository.Add(@event);

        UpdateEventHosts(@event, model.Hosts, [], chapterAdminMembers);

        var eventEmail = ScheduleEventEmail(@event, chapter, settings);

        if (@event.PublishedUtc != null)
        {
            _notificationService.AddNewEventNotifications(chapter, @event, venue, members, notificationSettings);
        }

        _unitOfWork.EventTopicRepository.AddMany(chapterTopics.Select(x => new EventTopic
        {
            EventId = @event.Id,
            TopicId = x.TopicId
        }));

        var shortcode = GenerateShortcode();
        while (await _unitOfWork.EventRepository.ShortcodeExists(shortcode).Run())
        {
            shortcode = GenerateShortcode();
        }

        @event.Shortcode = shortcode;

        await _unitOfWork.SaveChangesAsync();

        if (eventEmail?.ScheduledUtc != null)
        {
            eventEmail.JobId = _backgroundTaskService.Schedule(
                () => SendScheduledEmails(request, eventEmail.Id),
                eventEmail.ScheduledUtc.Value);
            _unitOfWork.EventEmailRepository.Update(eventEmail);
            await _unitOfWork.SaveChangesAsync();
        }

        if (@event.Ticketed)
        {
            _backgroundTaskService.Enqueue(() => _paymentService.EnsureProductExists(chapter.Id));
        }

        return ServiceResult.Successful();
    }

    public async Task DeleteEvent(MemberChapterAdminServiceRequest request, Guid id)
    {
        var (@event, eventEmail, responses) = await GetChapterAdminRestrictedContent(
            request,
            x => x.EventRepository.GetById(id),
            x => x.EventEmailRepository.GetByEventId(id),
            x => x.EventResponseRepository.GetByEventId(id));

        AssertEventCanBeDeleted(eventEmail, responses);

        _unitOfWork.EventRepository.Delete(@event);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Event> GetEvent(MemberChapterAdminServiceRequest request, Guid id)
    {
        var chapter = request.Chapter;

        var @event = await GetChapterAdminRestrictedContent(
            request,
            x => x.EventRepository.GetById(id));
        
        OdkAssertions.BelongsToChapter(@event, chapter.Id);
        
        return @event;
    }

    public async Task<EventAttendeesAdminPageViewModel> GetEventAttendeesViewModel(
        MemberChapterAdminServiceRequest request, Guid eventId)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (ownerSubscription,
            @event,
            responses,
            venue,
            members,
            memberSubscriptions,
            chapterMembershipSettings,
            chapterPrivacySettings,
            waitlist) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.EventRepository.GetById(eventId),
            x => x.EventResponseRepository.GetByEventId(eventId),
            x => x.VenueRepository.GetByEventId(eventId),
            x => x.MemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapter.Id),
            x => x.EventWaitlistMemberRepository.GetByEventId(eventId));

        OdkAssertions.BelongsToChapter(@event, chapter.Id);

        var responseDictionary = responses
            .ToDictionary(x => x.MemberId);

        var waitlistMemberIds = waitlist
            .Select(x => x.MemberId)
            .ToHashSet();

        var memberSubscriptionDictionary = memberSubscriptions
            .ToDictionary(x => x.MemberChapter.MemberId);

        return new EventAttendeesAdminPageViewModel
        {
            Chapter = chapter,
            Event = @event,
            Members = members
                .Where(x =>
                    responseDictionary.ContainsKey(x.Id) ||
                    waitlistMemberIds.Contains(x.Id) ||
                    _authorizationService.CanRespondToEvent(
                        @event,
                        x,
                        memberSubscriptionDictionary.ContainsKey(x.Id) ? memberSubscriptionDictionary[x.Id] : null,
                        chapterMembershipSettings,
                        chapterPrivacySettings))
                .ToArray(),
            OwnerSubscription = ownerSubscription,
            Platform = platform,
            Responses = responses,
            Venue = venue,
            Waitlist = waitlist
        };
    }

    public async Task<EventCreateAdminPageViewModel> GetEventCreateViewModel(
        MemberChapterAdminServiceRequest request)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (venues, adminMembers, eventSettings, currency, ownerSubscription) = await GetChapterAdminRestrictedContent(
            request,
            x => x.VenueRepository.GetByChapterId(chapter.Id),
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.ChapterEventSettingsRepository.GetByChapterId(chapter.Id),
            x => x.CurrencyRepository.GetByChapterId(chapter.Id),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id));

        return new EventCreateAdminPageViewModel
        {
            AdminMembers = adminMembers,
            Chapter = chapter,
            Currency = currency,
            Date = await GetNextAvailableEventDate(request),
            EventSettings = eventSettings,
            OwnerSubscription = ownerSubscription,
            Platform = platform,
            Venues = venues
        };
    }

    public async Task<EventEditAdminPageViewModel> GetEventEditViewModel(
        MemberChapterAdminServiceRequest request, Guid eventId)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var (
            ownerSubscription,
            @event,
            adminMembers,
            currency,
            hosts,
            venues
        ) = await _unitOfWork.RunAsync(            
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.EventRepository.GetById(eventId),
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.CurrencyRepository.GetByChapterId(chapter.Id),
            x => x.EventHostRepository.GetByEventId(eventId),
            x => x.VenueRepository.GetByChapterId(chapter.Id));

        AssertMemberIsChapterAdmin(
            request,
            adminMembers.FirstOrDefault(x => x.MemberId == currentMember.Id));

        OdkAssertions.BelongsToChapter(@event, chapter.Id);

        return new EventEditAdminPageViewModel
        {
            Chapter = chapter,
            ChapterAdminMembers = adminMembers,
            Currency = currency,
            Event = @event,
            Hosts = hosts,
            OwnerSubscription = ownerSubscription,
            Platform = platform,
            Venue = venues.First(x => x.Id == @event.VenueId),
            Venues = venues
        };
    }

    public async Task<EventInvitesAdminPageViewModel> GetEventInvitesViewModel(
        MemberChapterAdminServiceRequest request, Guid eventId)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (ownerSubscription, @event, eventEmail, members, invites, venue) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.EventRepository.GetById(eventId),
            x => x.EventEmailRepository.GetByEventId(eventId),
            x => x.MemberRepository.GetByChapterId(chapter.Id),
            x => x.EventInviteRepository.GetByEventId(eventId),
            x => x.VenueRepository.GetByEventId(eventId));

        OdkAssertions.BelongsToChapter(@event, chapter.Id);

        return new EventInvitesAdminPageViewModel
        {
            Chapter = chapter,
            Event = @event,
            Invites = new EventInvitesDto
            {
                EventId = eventId,
                ScheduledUtc = eventEmail?.ScheduledUtc,
                Sent = invites.Count,
                SentUtc = eventEmail?.SentUtc
            },
            Members = members,
            OwnerSubscription = ownerSubscription,
            Platform = platform,
            Venue = venue
        };
    }

    public async Task<EventsAdminPageViewModel> GetEventsAdminPageViewModel(
        MemberChapterAdminServiceRequest request, Chapter chapter, int page, int pageSize)
    {
        var platform = request.Platform;

        var eventSummaries = await GetChapterAdminRestrictedContent(
            request,
            x => x.EventRepository.GetSummariesByChapterId(chapter.Id, page, pageSize));

        return new EventsAdminPageViewModel
        {
            Chapter = chapter,
            Events = eventSummaries
        };
    }

    public async Task<EventSettingsAdminPageViewModel> GetEventSettingsViewModel(
        MemberChapterAdminServiceRequest request)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (ownerSubscription, settings) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterEventSettingsRepository.GetByChapterId(chapter.Id));

        return new EventSettingsAdminPageViewModel
        {
            Chapter = chapter,
            OwnerSubscription = ownerSubscription,
            Platform = platform,
            Settings = settings
        };
    }

    public async Task<EventTicketsAdminPageViewModel> GetEventTicketsViewModel(
        MemberChapterAdminServiceRequest request, Guid eventId)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (
            ownerSubscription,
            members,
            @event,
            venue,
            payments
        ) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.MemberRepository.GetAllByChapterId(chapter.Id),
            x => x.EventRepository.GetById(eventId),
            x => x.VenueRepository.GetByEventId(eventId),
            x => x.EventTicketPaymentRepository.GetConfirmedPayments(eventId));

        OdkAssertions.BelongsToChapter(@event, chapter.Id);

        var memberDictionary = members
            .ToDictionary(x => x.Id);

        var memberPayments = payments
            .GroupBy(x => x.Payment.MemberId)
            .ToDictionary(x => x.Key, x => x.Sum(y => y.Payment.Amount));

        return new EventTicketsAdminPageViewModel
        {
            Chapter = chapter,
            Event = @event,
            Payments = members
                .Where(x => memberPayments.ContainsKey(x.Id))
                .OrderBy(x => x.FullName)
                .Select(x => new MemberTicketPurchaseViewModel
                {
                    AmountPaid = memberPayments[x.Id],
                    AmountRemaining = (@event.TicketSettings?.Cost ?? 0) - memberPayments[x.Id],
                    Member = x
                })
                .ToArray(),
            OwnerSubscription = ownerSubscription,
            Platform = platform,
            Venue = venue
        };
    }

    public async Task<DateTime> GetNextAvailableEventDate(MemberChapterAdminServiceRequest request)
    {
        var chapter = request.Chapter;

        var startOfDay = chapter.CurrentTime().StartOfDay();
        var startOfDayUtc = chapter.FromLocalTime(startOfDay);

        var (events, settings) = await GetChapterAdminRestrictedContent(
            request,
            x => x.EventRepository.GetByChapterId(chapter.Id, startOfDayUtc),
            x => x.ChapterEventSettingsRepository.GetByChapterId(chapter.Id));

        return GetNextAvailableEventDate(chapter, settings, events);
    }

    public async Task PublishEvent(MemberChapterAdminServiceRequest request, Guid eventId)
    {
        var chapter = request.Chapter;

        var @event = await GetChapterAdminRestrictedContent(
            request,
            x => x.EventRepository.GetById(eventId));

        OdkAssertions.BelongsToChapter(@event, chapter.Id);

        if (@event.IsPublished)
        {
            return;
        }

        var (venue, members, notificationSettings) = await _unitOfWork.RunAsync(
            x => x.VenueRepository.GetById(@event.VenueId),
            x => x.MemberRepository.GetAllByChapterId(@event.ChapterId),
            x => x.MemberNotificationSettingsRepository.GetByChapterId(@event.ChapterId, NotificationType.NewEvent));

        @event.PublishedUtc = DateTime.UtcNow;
        _unitOfWork.EventRepository.Update(@event);

        _notificationService.AddNewEventNotifications(chapter, @event, venue, members, notificationSettings);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SendEventInviteeEmail(MemberChapterAdminServiceRequest request, Guid eventId,
        IEnumerable<EventResponseType> responseTypes, string subject, string body)
    {
        var chapter = request.Chapter;

        var (@event, members, responses, invites) = await GetChapterAdminRestrictedContent(
            request,
            x => x.EventRepository.GetById(eventId),
            x => x.MemberRepository.GetByChapterId(chapter.Id),
            x => x.EventResponseRepository.GetByEventId(eventId),
            x => x.EventInviteRepository.GetByEventId(eventId));

        OdkAssertions.BelongsToChapter(@event, chapter.Id);
        AssertEventEmailsCanBeSent(@event);

        responses = responses
            .Where(x => responseTypes.Contains(x.Type))
            .ToArray();

        var responseDictionary = responses.ToDictionary(x => x.MemberId, x => x);

        if (responseTypes.Contains(EventResponseType.None))
        {
            foreach (var invite in invites.Where(x => !responseDictionary.ContainsKey(x.MemberId)))
            {
                var response = new EventResponse
                {
                    EventId = eventId,
                    MemberId = invite.MemberId,
                    Type = EventResponseType.None
                };

                responseDictionary.Add(invite.MemberId, response);
            }
        }

        var to = members
            .Where(x => x.IsCurrent() && responseDictionary.ContainsKey(x.Id))
            .ToArray();

        await _memberEmailService.SendBulkEmail(request, chapter, to, subject, body);
    }

    public async Task<ServiceResult> SendEventInvites(
        MemberChapterAdminServiceRequest request, Guid eventId, bool test = false)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        var (
            venue,
            members,
            memberEmailPreferences,
            @event,
            eventEmail,
            responses,
            invites
        ) = await GetChapterAdminRestrictedContent(request,
            x => x.VenueRepository.GetByEventId(eventId),
            x => x.MemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberEmailPreferenceRepository.GetByChapterId(chapter.Id, MemberEmailPreferenceType.Events),
            x => x.EventRepository.GetById(eventId),
            x => x.EventEmailRepository.GetByEventId(eventId),
            x => x.EventResponseRepository.GetByEventId(eventId),
            x => x.EventInviteRepository.GetByEventId(eventId));

        OdkAssertions.BelongsToChapter(@event, chapter.Id);
        
        var validationResult = ValidateEventEmailCanBeSent(@event);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        if (!test && eventEmail?.SentUtc != null)
        {
            return ServiceResult.Failure("Invites have already been sent for this event");
        }

        if (test)
        {
            await _memberEmailService.SendEventInvites(request, chapter, @event, venue, [currentMember]);
            return ServiceResult.Successful();
        }

        var (membershipSettings, privacySettings, memberSubscriptions) = await _unitOfWork.RunAsync(
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapter.Id),
            x => x.MemberSubscriptionRepository.GetByChapterId(chapter.Id));

        return await SendEventInvites(
            request,
            chapter,
            @event,
            venue,
            membershipSettings,
            privacySettings,
            responses,
            invites,
            members,
            memberEmailPreferences,
            memberSubscriptions);
    }

    // Public for Hangfire
    public async Task SendScheduledEmails(ServiceRequest request, Guid eventEmailId)
    {
        await _loggingService.Info($"Sending event email {eventEmailId}");

        var email = await _unitOfWork.EventEmailRepository.GetByIdOrDefault(eventEmailId).Run();
        if (email == null)
        {
            await _loggingService.Info($"Error sending event email {eventEmailId}: not found");
            return;
        }

        var @event = await _unitOfWork.EventRepository.GetById(email.EventId).Run();
        if (!@event.IsPublished)
        {
            await _loggingService.Info($"Error sending event email {eventEmailId}: event not published");
            return;
        }

        var chapter = await _unitOfWork.ChapterRepository.GetById(@event.ChapterId).Run();

        if (@event.Date < chapter.CurrentTime().StartOfDay())
        {
            await _loggingService.Info($"Not sending event email {eventEmailId}: event is in the past");

            email.ScheduledUtc = null;
            _unitOfWork.EventEmailRepository.Update(email);
            await _unitOfWork.SaveChangesAsync();
            return;
        }

        var (
            ownerSubscription,
            membershipSettings,
            privacySettings,
            venue,
            responses,
            invites,
            members,
            memberEmailPreferences,
            memberSubscriptions
        ) = await _unitOfWork.RunAsync(
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(@event.ChapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(@event.ChapterId),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(@event.ChapterId),
            x => x.VenueRepository.GetById(@event.VenueId),
            x => x.EventResponseRepository.GetByEventId(@event.Id),
            x => x.EventInviteRepository.GetByEventId(@event.Id),
            x => x.MemberRepository.GetByChapterId(@event.ChapterId),
            x => x.MemberEmailPreferenceRepository.GetByChapterId(@event.ChapterId, MemberEmailPreferenceType.Events),
            x => x.MemberSubscriptionRepository.GetByChapterId(@event.ChapterId));

        if (ownerSubscription?.HasFeature(SiteFeatureType.ScheduledEventEmails) != true)
        {
            await _loggingService.Info($"Not sending event email {eventEmailId}: chapter does not have feature enabled");
            return;
        }

        try
        {
            await SendEventInvites(
                request,
                chapter,
                @event,
                venue,
                membershipSettings,
                privacySettings,
                responses,
                invites,
                members,
                memberEmailPreferences,
                memberSubscriptions);

            email.SentUtc = DateTime.UtcNow;
            _unitOfWork.EventEmailRepository.Update(email);
            await _unitOfWork.SaveChangesAsync();
        }
        catch
        {
            await _loggingService.Error("Error sending scheduled event emails");
        }
    }

    public async Task<ServiceResult> UpdateEvent(
        MemberChapterAdminServiceRequest request, Guid id, CreateEvent model)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        var (ownerSubscription,
            chapterAdminMembers,
            @event,
            hosts,
            venue,
            currency,
            attendees
        ) = await _unitOfWork.RunAsync(            
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.EventRepository.GetById(id),
            x => x.EventHostRepository.GetByEventId(id),
            x => x.VenueRepository.GetById(model.VenueId),
            x => x.CurrencyRepository.GetByChapterId(chapter.Id),
            x => x.EventResponseRepository.GetByEventId(id, EventResponseType.Yes));

        AssertMemberIsChapterAdmin(
            request,
            chapterAdminMembers.FirstOrDefault(x => x.MemberId == currentMember.Id));

        OdkAssertions.BelongsToChapter(@event, chapter.Id);

        if (model.AttendeeLimit < attendees.Count)
        {
            return ServiceResult.Failure(
                $"There are currently {attendees.Count} attendees - " +
                "you will need to reduce the number of attendees before reducing the limit");
        }

        var date = model.Date;
        if (date.TimeOfDay.TotalSeconds > 0)
        {
            date = chapter.FromLocalTime(date);
        }
        else
        {
            date = date.SpecifyKind(DateTimeKind.Utc);
        }

        var previousAttendeeLimit = @event.AttendeeLimit;

        @event.AttendeeLimit = model.AttendeeLimit;
        @event.Date = date;
        @event.Description = model.Description != null
            ? _htmlSanitizer.Sanitize(model.Description, DefaultHtmlSantizerOptions)
            : null;
        @event.EndTime = model.EndTime;
        @event.ImageUrl = model.ImageUrl;
        @event.IsPublic = model.IsPublic;
        @event.Name = model.Name;
        @event.RsvpDeadlineUtc = model.RsvpDeadline != null ? chapter.FromLocalTime(model.RsvpDeadline.Value) : null;
        @event.RsvpDisabled = model.RsvpDisabled;
        @event.Time = model.Time;
        @event.VenueId = model.VenueId;

        if (ownerSubscription?.HasFeature(SiteFeatureType.EventTickets) == true && model.TicketCost != null)
        {
            @event.TicketSettings ??= new EventTicketSettings
            {
                CurrencyId = currency.Id
            };

            @event.TicketSettings.Cost = Math.Round(model.TicketCost.Value, 2);
            @event.TicketSettings.Deposit = model.TicketDepositCost != null ? Math.Round(model.TicketDepositCost.Value, 2) : null;

            @event.WaitlistDisabled = @event.Ticketed;
        }
        else if (@event.TicketSettings != null)
        {
            _unitOfWork.EventTicketSettingsRepository.Delete(@event.TicketSettings);
            @event.TicketSettings = null;
        }

        var validationResult = ValidateEvent(@event, venue);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        UpdateEventHosts(@event, model.Hosts, hosts, chapterAdminMembers);

        _unitOfWork.EventRepository.Update(@event);

        var ticketSettings = @event.TicketSettings;
        if (ticketSettings != null)
        {
            if (model.TicketCost == null)
            {
                @event.TicketSettings = null;
                _unitOfWork.EventTicketSettingsRepository.Delete(ticketSettings);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        if (@event.Ticketed)
        {
            _backgroundTaskService.Enqueue(() => _paymentService.EnsureProductExists(chapter.Id));
        }

        if (@event.AttendeeLimit > previousAttendeeLimit)
        {
            _backgroundTaskService.Enqueue(() => _eventService.NotifyWaitlist(request, id));
        }

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> SetMissingEventShortcodes(MemberServiceRequest request)
    {
        var chapters = await GetSiteAdminRestrictedContent(request,
            x => x.ChapterRepository.GetAll());

        foreach (var chapter in chapters)
        {
            var events = await _unitOfWork.EventRepository.GetByChapterId(chapter.Id).Run();
            foreach (var @event in @events)
            {
                if (!string.IsNullOrEmpty(@event.Shortcode))
                {
                    continue;
                }

                @event.Shortcode = GenerateShortcode();
                _unitOfWork.EventRepository.Update(@event);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task UpdateEventSettings(
        MemberChapterAdminServiceRequest request, UpdateEventSettings model)
    {
        var chapter = request.Chapter;

        var (ownerSubscription, settings) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id),
            x => x.ChapterEventSettingsRepository.GetByChapterId(chapter.Id));

        settings ??= new();

        settings.DefaultDayOfWeek = model.DefaultDayOfWeek;
        settings.DefaultDescription = model.DefaultDescription;
        settings.DefaultEndTime = model.DefaultEndTime;
        settings.DefaultStartTime = model.DefaultStartTime;
        settings.DisableComments = model.DisableComments;

        if (ownerSubscription?.HasFeature(SiteFeatureType.ScheduledEventEmails) == true)
        {
            settings.DefaultScheduledEmailDayOfWeek = model.DefaultScheduledEmailDayOfWeek;
            settings.DefaultScheduledEmailTimeOfDay = model.DefaultScheduledEmailTimeOfDay;
        }
        else
        {
            settings.DefaultScheduledEmailDayOfWeek = null;
            settings.DefaultScheduledEmailTimeOfDay = null;
        }

        if (settings.ChapterId == default)
        {
            settings.ChapterId = chapter.Id;
            _unitOfWork.ChapterEventSettingsRepository.Add(settings);
        }
        else
        {
            _unitOfWork.ChapterEventSettingsRepository.Update(settings);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ServiceResult> UpdateMemberResponse(
        MemberChapterAdminServiceRequest request,
        Guid eventId,
        Guid memberId,
        EventResponseType responseType)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        await _loggingService.Info(
            $"Admin '{currentMember.Id}' updating member '{memberId}' " +
            $"response to '{responseType}' for event '{eventId}'");

        var (@event, member) = await GetChapterAdminRestrictedContent(
            request,
            x => x.EventRepository.GetById(eventId),
            x => x.MemberRepository.GetById(memberId));

        OdkAssertions.BelongsToChapter(@event, chapter.Id);

        var memberServiceRequest = MemberServiceRequest.Create(member, request);
        return await _eventService.UpdateMemberResponse(
            memberServiceRequest,
            eventId,
            responseType,
            adminMemberId: currentMember.Id);
    }

    public async Task<ServiceResult> UpdateScheduledEmail(
        MemberChapterAdminServiceRequest request, Guid eventId, DateTime? date)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        var (@event, eventEmail, ownerSubscription) = await GetChapterAdminRestrictedContent(
            request,
            x => x.EventRepository.GetById(eventId),
            x => x.EventEmailRepository.GetByEventId(eventId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id));

        OdkAssertions.BelongsToChapter(@event, chapter.Id);

        if (ownerSubscription?.HasFeature(SiteFeatureType.ScheduledEventEmails) != true)
        {
            return ServiceResult.Failure("Not permitted");
        }

        if (eventEmail?.SentUtc != null)
        {
            return ServiceResult.Failure("Email has already been sent");
        }

        // delete existing
        if (eventEmail != null)
        {
            _unitOfWork.EventEmailRepository.Delete(eventEmail);
            await _unitOfWork.SaveChangesAsync();

            if (!string.IsNullOrEmpty(eventEmail.JobId))
            {
                _backgroundTaskService.CancelJob(eventEmail.JobId);
            }

            eventEmail = null;
        }

        if (date == null)
        {
            return ServiceResult.Successful();
        }

        var scheduledUtc = chapter.FromLocalTime(date);
        if (scheduledUtc == null)
        {
            return ServiceResult.Failure("Scheduled date not set");
        }

        if (scheduledUtc > @event.Date)
        {
            return ServiceResult.Failure("Scheduled date cannot be after event");
        }

        if (scheduledUtc < DateTime.UtcNow)
        {
            return ServiceResult.Failure("Scheduled date cannot be in the past");
        }

        eventEmail = new EventEmail
        {
            EventId = eventId,
            ScheduledUtc = scheduledUtc
        };

        _unitOfWork.EventEmailRepository.Add(eventEmail);
        await _unitOfWork.SaveChangesAsync();

        var scheduledJobRequest = ServiceRequest.Create(request);

        eventEmail.JobId = _backgroundTaskService.Schedule(
            () => SendScheduledEmails(request, eventEmail.Id),
            scheduledUtc.Value);

        _unitOfWork.EventEmailRepository.Update(eventEmail);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    private static ServiceResult ValidateEventEmailCanBeSent(Event @event)
    {
        if (@event.Date < DateTime.UtcNow.Date)
        {
            return ServiceResult.Failure("Invites cannot be sent to past events");
        }

        return ServiceResult.Successful();
    }

    private static void AssertEventEmailsCanBeSent(Event @event)
    {
        var result = ValidateEventEmailCanBeSent(@event);
        if (!result.Success)
        {
            throw new OdkServiceException(result.Message ?? string.Empty);
        }
    }

    private static DateTime GetNextAvailableEventDate(
        Chapter chapter,
        ChapterEventSettings? settings,
        IReadOnlyCollection<Event> events)
    {
        var startOfDay = chapter.CurrentTime().StartOfDay();

        var startTime = settings?.DefaultStartTime ?? TimeSpan.Zero;

        if (settings?.DefaultDayOfWeek == null)
        {
            return startOfDay + startTime;
        }

        var nextEventDate = startOfDay.Next(settings.DefaultDayOfWeek.Value);

        if (events.Count == 0)
        {
            return nextEventDate + startTime;
        }

        var eventDatesLocal = events
            .Select(x => chapter.ToChapterTime(x.Date).Date)
            .ToArray();
        var lastEventDate = eventDatesLocal.Max();

        while (nextEventDate <= lastEventDate)
        {
            if (!eventDatesLocal.Contains(nextEventDate))
            {
                return nextEventDate + startTime;
            }

            nextEventDate = nextEventDate.AddDays(7);
        }

        return nextEventDate + startTime;
    }

    private static ServiceResult ValidateEvent(
        Event @event,
        Venue venue)
    {
        var messages = new List<string>();

        if (string.IsNullOrWhiteSpace(@event.Name))
        {
            messages.Add("Name is required");
        }

        if (@event.AttendeeLimit < 1)
        {
            messages.Add("Attendee limit cannot be less than 1");
        }

        if (@event.Date == DateTime.MinValue)
        {
            messages.Add("Date is required");
        }

        if (@event.RsvpDeadlineUtc >= @event.Date)
        {
            messages.Add("RSVP Deadline must be before event date");
        }

        if (venue.ChapterId != @event.ChapterId)
        {
            messages.Add("Venue not found");
        }

        var ticketSettings = @event.TicketSettings;
        if (ticketSettings != null)
        {
            if (ticketSettings.Cost <= 0)
            {
                messages.Add("Ticket cost must be greater than 0");
            }

            if (ticketSettings.Deposit <= 0)
            {
                messages.Add("Ticket deposit must be greater than 0");
            }

            if (ticketSettings.Cost < ticketSettings.Deposit)
            {
                messages.Add("Ticket cost cannot be less than the deposit");
            }
        }

        return messages.Count == 0
            ? ServiceResult.Successful()
            : ServiceResult.Failure(messages.First());
    }

    private void AssertEventCanBeDeleted(EventEmail? eventEmail, IReadOnlyCollection<EventResponse> responses)
    {
        if (eventEmail?.SentUtc != null)
        {
            throw new OdkServiceException("Events that have had invite emails sent cannot be deleted");
        }

        if (responses.Count > 0)
        {
            throw new OdkServiceException("Events with responses cannot be deleted");
        }
    }

    private string GenerateShortcode() => StringUtils.RandomString(_settings.ShortcodeLength);

    private EventEmail? ScheduleEventEmail(Event @event, Chapter chapter, ChapterEventSettings? settings)
    {
        if (settings?.DefaultScheduledEmailDayOfWeek == null)
        {
            return null;
        }

        var localEventDate = chapter.ToChapterTime(@event.Date).Date;
        var scheduledDate = localEventDate.Previous(settings.DefaultScheduledEmailDayOfWeek.Value);
        var scheduledDateTimeLocal = settings.GetScheduledDateTime(scheduledDate);
        if (scheduledDateTimeLocal == null)
        {
            return null;
        }

        var scheduledDateTimeUtc = chapter.TimeZone != null
            ? scheduledDateTimeLocal.Value.ToUtc(chapter.TimeZone)
            : scheduledDateTimeLocal.SpecifyKind(DateTimeKind.Utc);

        var eventEmail = new EventEmail
        {
            EventId = @event.Id,
            ScheduledUtc = scheduledDateTimeUtc
        };
        _unitOfWork.EventEmailRepository.Add(eventEmail);

        return eventEmail;
    }

    private async Task<ServiceResult> SendEventInvites(
        ServiceRequest request,
        Chapter chapter,
        Event @event,
        Venue venue,
        ChapterMembershipSettings? membershipSettings,
        ChapterPrivacySettings? privacySettings,
        IReadOnlyCollection<EventResponse> responses,
        IReadOnlyCollection<EventInvite> invites,
        IReadOnlyCollection<Member> members,
        IReadOnlyCollection<MemberEmailPreference> memberEmailPreferences,
        IReadOnlyCollection<MemberSubscription> memberSubscriptions)
    {
        var memberResponses = responses
            .ToDictionary(x => x.MemberId, x => x);
        var inviteDictionary = invites
            .ToDictionary(x => x.MemberId, x => x);
        var memberSubscriptionDictionary = memberSubscriptions
            .ToDictionary(x => x.MemberChapter.MemberId);

        var optOutMemberIds = memberEmailPreferences
            .Where(x => x.Type == MemberEmailPreferenceType.Events && x.Disabled)
            .Select(x => x.MemberId)
            .ToHashSet();

        var invitees = members
            .Where(x =>
                _authorizationService.CanRespondToEvent(
                    @event,
                    x,
                    memberSubscriptionDictionary.ContainsKey(x.Id) ? memberSubscriptionDictionary[x.Id] : null,
                    membershipSettings,
                    privacySettings) &&
                !optOutMemberIds.Contains(x.Id) &&
                !inviteDictionary.ContainsKey(x.Id) &&
                !memberResponses.ContainsKey(x.Id))
            .ToArray();

        await _memberEmailService.SendEventInvites(
            request,
            chapter,
            @event,
            venue,
            invitees);

        var sentDate = DateTime.UtcNow;

        var eventEmail = await _unitOfWork.EventEmailRepository.GetByEventId(@event.Id).Run();
        eventEmail ??= new();

        eventEmail.SentUtc = sentDate;

        if (eventEmail.EventId == default)
        {
            eventEmail.EventId = @event.Id;
            _unitOfWork.EventEmailRepository.Add(eventEmail);
        }
        else
        {
            _unitOfWork.EventEmailRepository.Update(eventEmail);
        }

        // Add null event responses to indicate that members have been invited
        var newInvites = invitees
            .Select(x => new EventInvite
            {
                EventId = @event.Id,
                MemberId = x.Id,
                SentUtc = sentDate
            });

        _unitOfWork.EventInviteRepository.AddMany(newInvites);

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    private void UpdateEventHosts(
        Event @event,
        IReadOnlyCollection<Guid> hosts,
        IReadOnlyCollection<EventHost> existingHosts,
        IReadOnlyCollection<ChapterAdminMember> adminMembers)
    {
        var adminMemberDictionary = adminMembers.ToDictionary(x => x.MemberId);
        var existingHostDictionary = existingHosts.ToDictionary(x => x.MemberId);

        foreach (var host in hosts)
        {
            if (!adminMemberDictionary.ContainsKey(host))
            {
                continue;
            }

            if (existingHostDictionary.ContainsKey(host))
            {
                continue;
            }

            _unitOfWork.EventHostRepository.Add(new EventHost
            {
                EventId = @event.Id,
                MemberId = host
            });
        }

        foreach (var existingHost in existingHosts)
        {
            if (!hosts.Contains(existingHost.MemberId))
            {
                _unitOfWork.EventHostRepository.Delete(existingHost);
            }
        }
    }
}