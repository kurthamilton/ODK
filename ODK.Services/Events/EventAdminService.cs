using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Extensions;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Platforms;
using ODK.Core.Utils;
using ODK.Core.Venues;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Chapters;
using ODK.Services.Emails;
using ODK.Services.Events.ViewModels;
using ODK.Services.Exceptions;
using ODK.Services.Notifications;

namespace ODK.Services.Events;

public class EventAdminService : OdkAdminServiceBase, IEventAdminService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IChapterUrlService _chapterUrlService;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    private readonly IPlatformProvider _platformProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUrlProvider _urlProvider;

    public EventAdminService(
        IUnitOfWork unitOfWork, 
        IEmailService emailService,
        IChapterUrlService chapterUrlService,
        IAuthorizationService authorizationService,
        IPlatformProvider platformProvider,
        IUrlProvider urlProvider,
        INotificationService notificationService)
        : base(unitOfWork)
    {
        _authorizationService = authorizationService;
        _chapterUrlService = chapterUrlService;
        _emailService = emailService;
        _notificationService = notificationService;
        _platformProvider = platformProvider;
        _unitOfWork = unitOfWork;
        _urlProvider = urlProvider;
    }

    public async Task<ServiceResult> CreateEvent(AdminServiceRequest request, CreateEvent model, bool draft)
    {
        var (currentMemberId, chapterId) = (request.CurrentMemberId, request.ChapterId);

        var (
            chapter, 
            ownerSubscription, 
            chapterAdminMembers, 
            currentMember, 
            venue, 
            settings, 
            chapterPaymentSettings,
            members,
            notificationSettings,
            chapterTopics
        ) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(chapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapterId),
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.VenueRepository.GetById(model.VenueId),
            x => x.ChapterEventSettingsRepository.GetByChapterId(chapterId),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetAllByChapterId(chapterId),
            x => x.MemberNotificationSettingsRepository.GetByChapterId(chapterId, NotificationType.NewEvent),
            x => x.ChapterTopicRepository.GetByChapterId(chapterId));

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        var date = Event.FromLocalTime(model.Date, chapter.TimeZone);
        var @event = new Event
        {
            AttendeeLimit = model.AttendeeLimit,
            ChapterId = chapterId,
            CreatedBy = currentMember.FullName,
            CreatedUtc = DateTime.UtcNow,
            Date = date,
            Description = model.Description,
            EndTime = model.EndTime,
            ImageUrl = model.ImageUrl,
            IsPublic = model.IsPublic,
            Name = model.Name,
            PublishedUtc = !draft ? DateTime.UtcNow : null,
            RsvpDeadlineUtc = model.RsvpDeadline != null ? chapter.FromLocalTime(model.RsvpDeadline.Value) : null,
            Time = model.Time,
            VenueId = model.VenueId
        };

        if (ownerSubscription?.HasFeature(SiteFeatureType.EventTickets) == true)
        {
            @event.TicketSettings = model.TicketCost != null ? new EventTicketSettings
            {
                Cost = Math.Round(model.TicketCost.Value, 2),
                Deposit = model.TicketDepositCost != null ? Math.Round(model.TicketDepositCost.Value, 2) : null,
            } : null;
        }

        var validationResult = ValidateEvent(@event, venue, chapterPaymentSettings);
        if (!validationResult.Success)
        {
            return validationResult;
        }        

        _unitOfWork.EventRepository.Add(@event);

        UpdateEventHosts(@event, model.Hosts, [], chapterAdminMembers);

        ScheduleEventEmail(@event, chapter, settings);
        
        if (@event.PublishedUtc != null)
        {
            _notificationService.AddNewEventNotifications(chapter, @event, venue, members, notificationSettings);
        }

        _unitOfWork.EventTopicRepository.AddMany(chapterTopics.Select(x => new EventTopic
        {
            EventId = @event.Id,
            TopicId = x.TopicId
        }));

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }
    
    public async Task DeleteEvent(AdminServiceRequest request, Guid id)
    {
        var (@event, eventEmail, responses) = await GetChapterAdminRestrictedContent(request,
            x => x.EventRepository.GetById(id),
            x => x.EventEmailRepository.GetByEventId(id),
            x => x.EventResponseRepository.GetByEventId(id));
        
        AssertEventCanBeDeleted(eventEmail, responses);

        _unitOfWork.EventRepository.Delete(@event);
        await _unitOfWork.SaveChangesAsync();
    }
    
    public async Task<Event> GetEvent(AdminServiceRequest request, Guid id)
    {
        var @event = await GetChapterAdminRestrictedContent(request,
            x => x.EventRepository.GetById(id));
        OdkAssertions.BelongsToChapter(@event, request.ChapterId);
        return @event;
    }
    
    public async Task<EventAttendeesAdminPageViewModel> GetEventAttendeesViewModel(AdminServiceRequest request, Guid eventId)
    {
        var platform = _platformProvider.GetPlatform();

        var (chapter, ownerSubscription, currentMember, @event, responses, venue, members) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(request.ChapterId),
            x => x.MemberRepository.GetById(request.CurrentMemberId),
            x => x.EventRepository.GetById(eventId),
            x => x.EventResponseRepository.GetByEventId(eventId),
            x => x.VenueRepository.GetByEventId(eventId),
            x => x.MemberRepository.GetByChapterId(request.ChapterId));

        OdkAssertions.BelongsToChapter(@event, request.ChapterId);

        return new EventAttendeesAdminPageViewModel
        {
            Chapter = chapter,
            CurrentMember = currentMember,
            Event = @event,
            Members = members,
            OwnerSubscription = ownerSubscription,
            Platform = platform,
            Responses = responses,
            Venue = venue
        };
    }

    public async Task<EventCreateAdminPageViewModel> GetEventCreateViewModel(AdminServiceRequest request)
    {
        var platform = _platformProvider.GetPlatform();

        var (chapter, venues, adminMembers, eventSettings, paymentSettings, ownerSubscription) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.VenueRepository.GetByChapterId(request.ChapterId),
            x => x.ChapterAdminMemberRepository.GetByChapterId(request.ChapterId),
            x => x.ChapterEventSettingsRepository.GetByChapterId(request.ChapterId),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(request.ChapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(request.ChapterId));

        return new EventCreateAdminPageViewModel
        {
            AdminMembers = adminMembers,
            Chapter = chapter,
            Date = await GetNextAvailableEventDate(request),
            EventSettings = eventSettings,
            OwnerSubscription = ownerSubscription,
            PaymentSettings = paymentSettings,
            Platform = platform,
            Venues = venues            
        };
    }

    public async Task<EventEditAdminPageViewModel> GetEventEditViewModel(AdminServiceRequest request, Guid eventId)
    {
        var platform = _platformProvider.GetPlatform();

        var (chapter, ownerSubscription, @event, currentMember, adminMembers, paymentSettings, hosts, venues) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(request.ChapterId),
            x => x.EventRepository.GetById(eventId),
            x => x.MemberRepository.GetById(request.CurrentMemberId),
            x => x.ChapterAdminMemberRepository.GetByChapterId(request.ChapterId),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(request.ChapterId),
            x => x.EventHostRepository.GetByEventId(eventId),
            x => x.VenueRepository.GetByChapterId(request.ChapterId));

        OdkAssertions.BelongsToChapter(@event, request.ChapterId);

        return new EventEditAdminPageViewModel
        {
            Chapter = chapter,
            ChapterAdminMembers = adminMembers,
            CurrentMember = currentMember,
            Event = @event,
            Hosts = hosts,
            OwnerSubscription = ownerSubscription,
            PaymentSettings = paymentSettings,
            Platform = platform,
            Venue = venues.First(x => x.Id == @event.VenueId),
            Venues = venues
        };
    }

    public async Task<EventInvitesAdminPageViewModel> GetEventInvitesViewModel(AdminServiceRequest request, Guid eventId)
    {
        var platform = _platformProvider.GetPlatform();

        var (chapter, ownerSubscription, currentMember, @event, eventEmail, members, invites, venue) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(request.ChapterId),
            x => x.MemberRepository.GetById(request.CurrentMemberId),
            x => x.EventRepository.GetById(eventId),
            x => x.EventEmailRepository.GetByEventId(eventId),
            x => x.MemberRepository.GetByChapterId(request.ChapterId),
            x => x.EventInviteRepository.GetByEventId(eventId),
            x => x.VenueRepository.GetByEventId(eventId));

        OdkAssertions.BelongsToChapter(@event, request.ChapterId);

        return new EventInvitesAdminPageViewModel
        {
            Chapter = chapter,
            CurrentMember = currentMember,
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

    public async Task<EventResponsesDto> GetEventResponsesDto(AdminServiceRequest request, Guid eventId)
    {
        var (@event, responses, members) = await GetChapterAdminRestrictedContent(request,
            x => x.EventRepository.GetById(eventId),
            x => x.EventResponseRepository.GetByEventId(eventId),
            x => x.MemberRepository.GetByChapterId(request.ChapterId));

        OdkAssertions.BelongsToChapter(@event, request.ChapterId);

        return new EventResponsesDto
        {
            Members = members,
            Responses = responses
        };
    }

    public async Task<EventsAdminPageViewModel> GetEventsDto(AdminServiceRequest request, int page, int pageSize)
    {
        var platform = _platformProvider.GetPlatform();

        var (chapter, events) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.EventRepository.GetByChapterId(request.ChapterId, page, pageSize));

        var eventIds = events
            .Select(x => x.Id)
            .ToArray();

        var venueIds = events
            .Select(x => x.VenueId)
            .Distinct()
            .ToArray();

        var (venues, invites, responses, emails) = await _unitOfWork.RunAsync(
            x => x.VenueRepository.GetByChapterId(request.ChapterId, venueIds),
            x => x.EventInviteRepository.GetEventInvitesDtos(eventIds),
            x => x.EventResponseRepository.GetResponseSummaries(eventIds),
            x => x.EventEmailRepository.GetByEventIds(eventIds));

        return new EventsAdminPageViewModel
        {
            Chapter = chapter,
            Events = events,    
            Invites = GetEventInvitesDtos(eventIds, invites, emails),
            Platform = platform,
            Responses = responses,
            Venues = venues
        };
    }

    public async Task<IReadOnlyCollection<Event>> GetEventsByVenue(AdminServiceRequest request, Guid venueId)
    {
        var (venue, events) = await GetChapterAdminRestrictedContent(request,
            x => x.VenueRepository.GetById(venueId),
            x => x.EventRepository.GetByVenueId(venueId));

        OdkAssertions.BelongsToChapter(venue, request.ChapterId);

        return events;
    }
    
    public async Task<EventSettingsAdminPageViewModel> GetEventSettingsViewModel(AdminServiceRequest request)
    {
        var platform = _platformProvider.GetPlatform();

        var (chapter, ownerSubscription, settings) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(request.ChapterId),
            x => x.ChapterEventSettingsRepository.GetByChapterId(request.ChapterId));

        return new EventSettingsAdminPageViewModel
        {
            Chapter = chapter,
            OwnerSubscription = ownerSubscription,
            Platform = platform,
            Settings = settings
        };
    }

    public async Task<EventTicketsAdminPageViewModel> GetEventTicketsViewModel(AdminServiceRequest request, Guid eventId)
    {
        var platform = _platformProvider.GetPlatform();

        var (chapter, ownerSubscription, currentMember, @event, venue, paymentSettings, purchases) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(request.ChapterId),
            x => x.MemberRepository.GetById(request.CurrentMemberId),
            x => x.EventRepository.GetById(eventId),
            x => x.VenueRepository.GetByEventId(eventId),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(request.ChapterId),
            x => x.EventTicketPurchaseRepository.GetByEventId(eventId));

        OdkAssertions.BelongsToChapter(@event, request.ChapterId);

        return new EventTicketsAdminPageViewModel
        {
            Chapter = chapter,
            CurrentMember = currentMember,
            Event = @event,
            OwnerSubscription = ownerSubscription,
            PaymentSettings = paymentSettings,
            Platform = platform,
            Purchases = purchases,
            Venue = venue
        };
    }

    public async Task<DateTime> GetNextAvailableEventDate(AdminServiceRequest request)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetById(request.ChapterId).Run();

        var startOfDay = chapter.CurrentTime().StartOfDay();
        var startOfDayUtc = chapter.FromLocalTime(startOfDay);

        var (events, settings) = await GetChapterAdminRestrictedContent(request,
            x => x.EventRepository.GetByChapterId(request.ChapterId, startOfDayUtc),
            x => x.ChapterEventSettingsRepository.GetByChapterId(request.ChapterId));

        return GetNextAvailableEventDate(chapter, settings, events);
    }

    public async Task PublishEvent(AdminServiceRequest request, Guid eventId)
    {
        var @event = await GetChapterAdminRestrictedContent(request, 
            x => x.EventRepository.GetById(eventId));

        OdkAssertions.BelongsToChapter(@event, request.ChapterId);

        if (@event.IsPublished)
        {
            return;
        }

        var (chapter, venue, members, notificationSettings) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(@event.ChapterId),
            x => x.VenueRepository.GetById(@event.VenueId),
            x => x.MemberRepository.GetAllByChapterId(@event.ChapterId),
            x => x.MemberNotificationSettingsRepository.GetByChapterId(@event.ChapterId, NotificationType.NewEvent));

        @event.PublishedUtc = DateTime.UtcNow;
        _unitOfWork.EventRepository.Update(@event);

        _notificationService.AddNewEventNotifications(chapter, @event, venue, members, notificationSettings);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SendEventInviteeEmail(AdminServiceRequest request, Guid eventId, 
        IEnumerable<EventResponseType> responseTypes, string subject, string body)
    {
        var (chapter, chapterAdminMember, @event, members, responses, invites) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.ChapterAdminMemberRepository.GetByMemberId(request.CurrentMemberId, request.ChapterId),
            x => x.EventRepository.GetById(eventId),
            x => x.MemberRepository.GetByChapterId(request.ChapterId),
            x => x.EventResponseRepository.GetByEventId(eventId),
            x => x.EventInviteRepository.GetByEventId(eventId));

        OdkAssertions.BelongsToChapter(@event, request.ChapterId);
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

        await _emailService.SendBulkEmail(chapterAdminMember, chapter, to, subject, body);
    }

    public async Task<ServiceResult> SendEventInvites(AdminServiceRequest request, Guid eventId, bool test = false)
    {
        var (
            chapterAdminMembers, 
            currentMember, 
            chapter, 
            venue, 
            members, 
            memberEmailPreferences, 
            @event, 
            eventEmail, 
            responses, 
            invites
        ) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(request.ChapterId),
            x => x.MemberRepository.GetById(request.CurrentMemberId),
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.VenueRepository.GetByEventId(eventId),
            x => x.MemberRepository.GetByChapterId(request.ChapterId),
            x => x.MemberEmailPreferenceRepository.GetByChapterId(request.ChapterId, MemberEmailPreferenceType.Events),
            x => x.EventRepository.GetById(eventId),
            x => x.EventEmailRepository.GetByEventId(eventId),
            x => x.EventResponseRepository.GetByEventId(eventId),
            x => x.EventInviteRepository.GetByEventId(eventId));

        OdkAssertions.BelongsToChapter(@event, request.ChapterId);
        AssertMemberIsChapterAdmin(currentMember, request.ChapterId, chapterAdminMembers);

        var validationResult = ValidateEventEmailCanBeSent(@event);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        if (!test && eventEmail?.SentUtc != null)
        {
            return ServiceResult.Failure("Invites have already been sent for this event");
        }        

        var chapterAdminMember = chapterAdminMembers.First(x => x.MemberId == request.CurrentMemberId);

        var parameters = GetEventEmailParameters(chapter, @event, venue);

        if (test)
        {            
            await _emailService.SendEmail(chapter, currentMember.ToEmailAddressee(), EmailType.EventInvite, parameters);
            return ServiceResult.Successful();
        }

        var (membershipSettings, privacySettings, memberSubscriptions) = await _unitOfWork.RunAsync(
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapter.Id),
            x => x.MemberSubscriptionRepository.GetByChapterId(chapter.Id));

        return await SendEventInvites(
            chapterAdminMember, 
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

    public async Task SendScheduledEmails()
    {
        var (chapters, emails) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetAll(),
            x => x.EventEmailRepository.GetScheduled());

        if (emails.Count == 0)
        {
            return;
        }

        var chapterDictionary = chapters
            .ToDictionary(x => x.Id);

        var eventIds = emails
            .Select(x => x.EventId)
            .ToArray();

        var events = await _unitOfWork.EventRepository.GetByIds(eventIds).Run();

        var emailDictionary = emails.ToDictionary(x => x.EventId);

        foreach (var @event in events)
        {
            if (!@event.IsPublished)
            {
                continue;
            }

            var email = emailDictionary[@event.Id];
            var chapter = chapterDictionary[@event.ChapterId];

            if (@event.Date < chapter.CurrentTime().StartOfDay())
            {
                email.ScheduledUtc = null;
                _unitOfWork.EventEmailRepository.Update(email);
                await _unitOfWork.SaveChangesAsync();
                continue;
            }

            if (email.ScheduledUtc > DateTime.Now)
            {
                continue;
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
                continue;
            }

            try
            {
                await SendEventInvites(
                    chapterAdminMember: null, 
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
            catch
            {
                // do nothing
            }
        }
    }

    public async Task<ServiceResult> UpdateEvent(AdminServiceRequest request, Guid id, CreateEvent model)
    {
        var (chapter, ownerSubscription, chapterAdminMembers, currentMember, @event, hosts, venue, chapterPaymentSettings) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(request.ChapterId),
            x => x.ChapterAdminMemberRepository.GetByChapterId(request.ChapterId),
            x => x.MemberRepository.GetById(request.CurrentMemberId),
            x => x.EventRepository.GetById(id),
            x => x.EventHostRepository.GetByEventId(id),
            x => x.VenueRepository.GetById(model.VenueId),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(request.ChapterId));

        OdkAssertions.BelongsToChapter(@event, request.ChapterId);

        var date = model.Date;
        if (date.TimeOfDay.TotalSeconds > 0)
        {
            date = chapter.FromLocalTime(date);
        }
        else
        {
            date = date.SpecifyKind(DateTimeKind.Utc);
        }

        @event.AttendeeLimit = model.AttendeeLimit;
        @event.Date = date;
        @event.Description = model.Description;
        @event.EndTime = model.EndTime;
        @event.ImageUrl = model.ImageUrl;
        @event.IsPublic = model.IsPublic;
        @event.Name = model.Name;
        @event.RsvpDeadlineUtc = model.RsvpDeadline != null ? chapter.FromLocalTime(model.RsvpDeadline.Value) : null;
        @event.Time = model.Time;
        @event.VenueId = model.VenueId;

        if (ownerSubscription?.HasFeature(SiteFeatureType.EventTickets) == true && model.TicketCost != null)
        {
            @event.TicketSettings ??= new EventTicketSettings();
            @event.TicketSettings.Cost = Math.Round(model.TicketCost.Value, 2);
            @event.TicketSettings.Deposit = model.TicketDepositCost != null ? Math.Round(model.TicketDepositCost.Value, 2) : null;
        }
        else if (@event.TicketSettings != null)
        {
            _unitOfWork.EventTicketSettingsRepository.Delete(@event.TicketSettings);
            @event.TicketSettings = null;
        }
        
        var validationResult = ValidateEvent(@event, venue, chapterPaymentSettings);
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

        return ServiceResult.Successful();
    }

    public async Task UpdateEventSettings(AdminServiceRequest request, UpdateEventSettings model)
    {
        var (ownerSubscription, settings) = await GetChapterAdminRestrictedContent(request,
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(request.ChapterId),
            x => x.ChapterEventSettingsRepository.GetByChapterId(request.ChapterId));

        if (settings == null)
        {
            settings = new();
        }

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
            settings.ChapterId = request.ChapterId;
            _unitOfWork.ChapterEventSettingsRepository.Add(settings);
        }
        else
        {
            _unitOfWork.ChapterEventSettingsRepository.Update(settings);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<EventResponse> UpdateMemberResponse(AdminServiceRequest request, Guid eventId, Guid memberId, 
        EventResponseType responseType)
    {
        var (@event, response) = await GetChapterAdminRestrictedContent(request,
            x => x.EventRepository.GetById(eventId),
            x => x.EventResponseRepository.GetByMemberId(memberId, eventId));

        OdkAssertions.BelongsToChapter(@event, request.ChapterId);

        if (response == null)
        {
            response = new EventResponse
            {
                EventId = eventId,
                MemberId = memberId,
                Type = responseType
            };
            _unitOfWork.EventResponseRepository.Add(response);
        }
        else
        {
            response.Type = responseType;
            _unitOfWork.EventResponseRepository.Update(response);
        }

        await _unitOfWork.SaveChangesAsync();

        return response;
    }

    public async Task<ServiceResult> UpdateScheduledEmail(AdminServiceRequest request, Guid eventId, DateTime? date)
    {
        var (chapter, chapterAdminMembers, currentMember, @event, eventEmail, ownerSubscription) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.ChapterAdminMemberRepository.GetByChapterId(request.ChapterId),
            x => x.MemberRepository.GetById(request.CurrentMemberId),
            x => x.EventRepository.GetById(eventId),
            x => x.EventEmailRepository.GetByEventId(eventId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(request.ChapterId));

        OdkAssertions.BelongsToChapter(@event, request.ChapterId);
        
        if (ownerSubscription?.HasFeature(SiteFeatureType.ScheduledEventEmails) != true)
        {
            return ServiceResult.Failure("Not permitted");
        }

        if (eventEmail?.SentUtc != null)
        {
            return ServiceResult.Failure("Email has already been sent");
        }

        if (eventEmail == null)
        {
            eventEmail = new EventEmail();
        }
        else if (date == null)
        {
            _unitOfWork.EventEmailRepository.Delete(eventEmail);
            await _unitOfWork.SaveChangesAsync();
            return ServiceResult.Successful();
        }

        var scheduledUtc = chapter.FromLocalTime(date);
        if (scheduledUtc > @event.Date)
        {
            return ServiceResult.Failure("Scheduled date cannot be after event");
        }

        if (scheduledUtc < DateTime.UtcNow)
        {
            return ServiceResult.Failure("Scheduled date cannot be in the past");
        }

        eventEmail.ScheduledUtc = scheduledUtc;

        if (eventEmail.EventId == default)
        {
            eventEmail.EventId = eventId;
            _unitOfWork.EventEmailRepository.Add(eventEmail);
        }
        else
        {
            _unitOfWork.EventEmailRepository.Update(eventEmail);
        }

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
            throw new OdkServiceException(result.Message ?? "");
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
        Venue venue, 
        ChapterPaymentSettings? chapterPaymentSettings)
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

            if (chapterPaymentSettings == null)
            {
                messages.Add("Cannot setup tickets before payment settings have been set up");
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

    private IDictionary<string, string> GetEventEmailParameters(Chapter chapter, Event @event, Venue venue)
    {
        var time = @event.ToLocalTimeString(chapter.TimeZone);

        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"event.date", @event.Date.ToString("dddd dd MMMM, yyyy")},
            {"event.id", @event.Id.ToString()},
            {"event.location", venue.Name},
            {"event.name", @event.Name},
            {"event.time", time}
        };

        var eventUrl = _urlProvider.EventUrl(chapter, @event.Id);
        var rsvpUrl = _urlProvider.EventRsvpUrl(chapter, @event.Id, EventResponseType.Yes);
        var unsubscribeUrl = _urlProvider.EmailPreferences(chapter);

        parameters.Add("event.rsvpurl", rsvpUrl);
        parameters.Add("event.url", eventUrl);
        parameters.Add("unsubscribeUrl", unsubscribeUrl);

        return parameters;
    }

    private IReadOnlyCollection<EventInvitesDto> GetEventInvitesDtos(IEnumerable<Guid> eventIds,
        IEnumerable<EventInviteSummaryDto> invites, IEnumerable<EventEmail> emails)
    {
        var eventDictionary = emails
            .ToDictionary(x => x.EventId);
        var invitesDictionary = invites
            .ToDictionary(x => x.EventId, x => x.Sent);

        return eventIds.Select(x => new EventInvitesDto
        {
            EventId = x,
            Sent = invitesDictionary.ContainsKey(x) ? invitesDictionary[x] : 0,
            SentUtc = eventDictionary.ContainsKey(x) ? eventDictionary[x].SentUtc : default,
            ScheduledUtc = eventDictionary.ContainsKey(x) ? eventDictionary[x].ScheduledUtc : default,
        }).ToArray();
    }

    private void ScheduleEventEmail(Event @event, Chapter chapter, ChapterEventSettings? settings)
    {
        if (settings?.DefaultScheduledEmailDayOfWeek == null)
        {
            return;
        }

        var scheduledDate = @event.Date.Previous(settings.DefaultScheduledEmailDayOfWeek.Value);
        var scheduledDateTimeLocal = settings.GetScheduledDateTime(scheduledDate);
        if (scheduledDateTimeLocal == null)
        {
            return;
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
    }

    private async Task<ServiceResult> SendEventInvites(
        ChapterAdminMember? chapterAdminMember, 
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
        var parameters = GetEventEmailParameters(chapter, @event, venue);

        var memberResponses = responses.ToDictionary(x => x.MemberId, x => x);
        var inviteDictionary = invites.ToDictionary(x => x.MemberId, x => x);
        var memberSubscriptionDictionary = memberSubscriptions.ToDictionary(x => x.MemberId);

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

        await _emailService.SendBulkEmail(
            chapterAdminMember,
            chapter,
            invitees,
            EmailType.EventInvite,
            parameters);

        var sentDate = DateTime.UtcNow;

        var eventEmail = await _unitOfWork.EventEmailRepository.GetByEventId(@event.Id).Run();
        if (eventEmail == null)
        {
            eventEmail = new();
        }

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
