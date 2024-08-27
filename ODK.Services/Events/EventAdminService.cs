using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Extensions;
using ODK.Core.Members;
using ODK.Core.Utils;
using ODK.Core.Venues;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Chapters;
using ODK.Services.Emails;
using ODK.Services.Events.ViewModels;
using ODK.Services.Exceptions;

namespace ODK.Services.Events;

public class EventAdminService : OdkAdminServiceBase, IEventAdminService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IChapterUrlService _chapterUrlService;
    private readonly IEmailService _emailService;    
    private readonly EventAdminServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public EventAdminService(
        IUnitOfWork unitOfWork, 
        EventAdminServiceSettings settings, 
        IEmailService emailService,
        IChapterUrlService chapterUrlService,
        IAuthorizationService authorizationService)
        : base(unitOfWork)
    {
        _authorizationService = authorizationService;
        _chapterUrlService = chapterUrlService;
        _emailService = emailService;
        _settings = settings;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> CreateEvent(AdminServiceRequest request, CreateEvent model, bool draft)
    {
        var (currentMemberId, chapterId) = (request.CurrentMemberId, request.ChapterId);

        var (chapter, chapterAdminMembers, currentMember, venue, settings, chapterPaymentSettings) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(chapterId),
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.VenueRepository.GetById(model.VenueId),
            x => x.ChapterEventSettingsRepository.GetByChapterId(chapterId),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterId));

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
            TicketSettings = model.TicketCost != null ? new EventTicketSettings
            {
                Cost = Math.Round(model.TicketCost.Value, 2),
                Deposit = model.TicketDepositCost != null ? Math.Round(model.TicketDepositCost.Value, 2) : null,
            } : null,
            Time = model.Time,
            VenueId = model.VenueId
        };

        var validationResult = ValidateEvent(@event, venue, chapterPaymentSettings);
        if (!validationResult.Success)
        {
            return validationResult;
        }        

        _unitOfWork.EventRepository.Add(@event);

        UpdateEventHosts(@event, model.Hosts, [], chapterAdminMembers);

        ScheduleEventEmail(@event, chapter, settings);
        
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
    
    public async Task<IReadOnlyCollection<EventInvitesDto>> GetChapterInvites(AdminServiceRequest request,
        IReadOnlyCollection<Guid> eventIds)
    {
        if (eventIds.Count == 0)
        {
            return Array.Empty<EventInvitesDto>();
        }

        var (invites, emails) = await GetChapterAdminRestrictedContent(request,
            x => x.EventInviteRepository.GetEventInvitesDtos(eventIds),
            x => x.EventEmailRepository.GetByEventIds(eventIds));

        return GetEventInvitesDtos(eventIds, invites, emails);
    }
    
    public async Task<IReadOnlyCollection<EventResponse>> GetChapterResponses(AdminServiceRequest request, 
        IReadOnlyCollection<Guid> eventIds)
    {
        if (eventIds.Count == 0)
        {
            return Array.Empty<EventResponse>();
        }

        return await GetChapterAdminRestrictedContent(request,
            x => x.EventResponseRepository.GetByEventIds(eventIds));
    }

    public async Task<Event> GetEvent(AdminServiceRequest request, Guid id)
    {
        var @event = await GetChapterAdminRestrictedContent(request,
            x => x.EventRepository.GetById(id));
        OdkAssertions.BelongsToChapter(@event, request.ChapterId);
        return @event;
    }
    
    public async Task<EventCreateViewModel> GetEventCreateViewModel(AdminServiceRequest request)
    {
        var (chapter, venues, adminMembers, eventSettings, paymentSettings) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.VenueRepository.GetByChapterId(request.ChapterId),
            x => x.ChapterAdminMemberRepository.GetByChapterId(request.ChapterId),
            x => x.ChapterEventSettingsRepository.GetByChapterId(request.ChapterId),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(request.ChapterId));

        return new EventCreateViewModel
        {
            AdminMembers = adminMembers,
            Chapter = chapter,
            Date = await GetNextAvailableEventDate(request),
            EventSettings = eventSettings,
            PaymentSettings = paymentSettings,
            Venues = venues            
        };
    }

    public async Task<IReadOnlyCollection<EventHost>> GetEventHosts(AdminServiceRequest request, Guid eventId)
    {
        var (@event, hosts) = await GetChapterAdminRestrictedContent(request,
            x => x.EventRepository.GetById(eventId),
            x => x.EventHostRepository.GetByEventId(eventId));

        OdkAssertions.BelongsToChapter(@event, request.ChapterId);

        return hosts;
    }

    public async Task<EventInvitesDto> GetEventInvites(AdminServiceRequest request, Guid eventId)
    {
        var (@event, invites, eventEmail) = await GetChapterAdminRestrictedContent(request,
            x => x.EventRepository.GetById(eventId),
            x => x.EventInviteRepository.GetByEventId(eventId),
            x => x.EventEmailRepository.GetByEventId(eventId));

        OdkAssertions.BelongsToChapter(@event, request.ChapterId);

        return new EventInvitesDto
        {
            EventId = eventId,
            Sent = invites.Count,
            SentUtc = eventEmail?.SentUtc,
            ScheduledUtc = eventEmail?.ScheduledUtc,
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

    public async Task<IReadOnlyCollection<Event>> GetEvents(AdminServiceRequest request, int page, int pageSize)
    {
        return await GetChapterAdminRestrictedContent(request,
            x => x.EventRepository.GetByChapterId(request.ChapterId, page, pageSize));
    }

    public async Task<EventsDto> GetEventsDto(AdminServiceRequest request, int page, int pageSize)
    {
        var events = await GetChapterAdminRestrictedContent(request,
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

        return new EventsDto
        {
            Events = events,    
            Invites = GetEventInvitesDtos(eventIds, invites, emails),
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
    
    public async Task<IReadOnlyCollection<EventTicketPurchase>> GetEventTicketPurchases(AdminServiceRequest request, Guid eventId)
    {
        var (@event, ticketPurchases) = await GetChapterAdminRestrictedContent(request,
            x => x.EventRepository.GetById(eventId),
            x => x.EventTicketPurchaseRepository.GetByEventId(eventId));

        OdkAssertions.BelongsToChapter(@event, request.ChapterId);

        return ticketPurchases;
    }

    public async Task<DateTime> GetNextAvailableEventDate(AdminServiceRequest request)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetById(request.ChapterId).RunAsync();

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

        @event.PublishedUtc = DateTime.UtcNow;
        _unitOfWork.EventRepository.Update(@event);
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
            .Where(x => x.IsCurrent() && x.EmailOptIn && responseDictionary.ContainsKey(x.Id))
            .ToArray();

        await _emailService.SendBulkEmail(chapterAdminMember, chapter, to, subject, body);
    }

    public async Task<ServiceResult> SendEventInvites(AdminServiceRequest request, Guid eventId, bool test = false)
    {
        var (chapterAdminMembers, currentMember, chapter, venue, members, @event, eventEmail, responses, invites) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(request.ChapterId),
            x => x.MemberRepository.GetById(request.CurrentMemberId),
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.VenueRepository.GetByEventId(eventId),
            x => x.MemberRepository.GetByChapterId(request.ChapterId),
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

        var events = await _unitOfWork.EventRepository.GetByIds(eventIds).RunAsync();

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

            var (membershipSettings, privacySettings, venue, responses, invites, members, memberSubscriptions) = await _unitOfWork.RunAsync(
                x => x.ChapterMembershipSettingsRepository.GetByChapterId(@event.ChapterId),
                x => x.ChapterPrivacySettingsRepository.GetByChapterId(@event.ChapterId),
                x => x.VenueRepository.GetById(@event.VenueId),
                x => x.EventResponseRepository.GetByEventId(@event.Id),
                x => x.EventInviteRepository.GetByEventId(@event.Id),
                x => x.MemberRepository.GetByChapterId(@event.ChapterId),
                x => x.MemberSubscriptionRepository.GetByChapterId(@event.ChapterId));

            try
            {
                await SendEventInvites(null, chapter, @event, venue, membershipSettings, privacySettings, responses, invites, members, memberSubscriptions);
            }            
            catch
            {
                // do nothing
            }
        }
    }

    public async Task<ServiceResult> UpdateEvent(AdminServiceRequest request, Guid id, CreateEvent model)
    {
        var (chapter, chapterAdminMembers, currentMember, @event, hosts, venue, chapterPaymentSettings) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
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

        if (model.TicketCost != null)
        {
            @event.TicketSettings ??= new EventTicketSettings();
            @event.TicketSettings.Cost = Math.Round(model.TicketCost.Value, 2);
            @event.TicketSettings.Deposit = model.TicketDepositCost != null ? Math.Round(model.TicketDepositCost.Value, 2) : null;
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
        var (chapter, chapterAdminMembers, currentMember, @event, eventEmail) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.ChapterAdminMemberRepository.GetByChapterId(request.ChapterId),
            x => x.MemberRepository.GetById(request.CurrentMemberId),
            x => x.EventRepository.GetById(eventId),
            x => x.EventEmailRepository.GetByEventId(eventId));

        OdkAssertions.BelongsToChapter(@event, request.ChapterId);
        
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

        while (nextEventDate < lastEventDate)
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

        parameters.Add("event.rsvpurl", _chapterUrlService.GetChapterUrl(chapter, _settings.EventRsvpUrlFormat, parameters));
        parameters.Add("event.url", _chapterUrlService.GetChapterUrl(chapter, _settings.EventUrlFormat, parameters));
        parameters.Add("unsubscribeUrl", _chapterUrlService.GetChapterUrl(chapter, _settings.UnsubscribeUrlFormat, parameters));

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
        IReadOnlyCollection<MemberSubscription> memberSubscriptions)
    {
        var parameters = GetEventEmailParameters(chapter, @event, venue);

        var memberResponses = responses.ToDictionary(x => x.MemberId, x => x);
        var inviteDictionary = invites.ToDictionary(x => x.MemberId, x => x);
        var memberSubscriptionDictionary = memberSubscriptions.ToDictionary(x => x.MemberId);

        var invited = members
            .Where(x => 
                _authorizationService.CanRespondToEvent(
                    @event, 
                    x, 
                    memberSubscriptionDictionary.ContainsKey(x.Id) ? memberSubscriptionDictionary[x.Id] : null,
                    membershipSettings,
                    privacySettings) && 
                x.EmailOptIn && 
                !inviteDictionary.ContainsKey(x.Id) && 
                !memberResponses.ContainsKey(x.Id))
            .ToArray();

        await _emailService.SendBulkEmail(
            chapterAdminMember,
            chapter,
            invited,
            EmailType.EventInvite,
            parameters);

        var sentDate = DateTime.UtcNow;

        var eventEmail = await _unitOfWork.EventEmailRepository.GetByEventId(@event.Id).RunAsync();
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
        var newInvites = invited
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
