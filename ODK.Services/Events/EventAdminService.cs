using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Utils;
using ODK.Core.Venues;
using ODK.Data.Core;
using ODK.Services.Emails;
using ODK.Services.Exceptions;

namespace ODK.Services.Events;

public class EventAdminService : OdkAdminServiceBase, IEventAdminService
{
    private readonly IEmailService _emailService;
    private readonly EventAdminServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public EventAdminService(IUnitOfWork unitOfWork, EventAdminServiceSettings settings, IEmailService emailService)
        : base(unitOfWork)
    {
        _emailService = emailService;
        _settings = settings;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> CreateEvent(Guid currentMemberId, CreateEvent model)
    {
        var (chapter, chapterAdminMembers, currentMember, venue, settings) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(model.ChapterId),
            x => x.ChapterAdminMemberRepository.GetByChapterId(model.ChapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.VenueRepository.GetById(model.VenueId),
            x => x.ChapterEventSettingsRepository.GetByChapterId(model.ChapterId));

        AssertMemberIsChapterAdmin(currentMember, model.ChapterId, chapterAdminMembers);
        
        var @event = new Event
        {
            ChapterId = model.ChapterId,
            CreatedBy = currentMember.FullName,
            Date = model.Date.SpecifyKind(DateTimeKind.Utc),
            Description = model.Description,
            ImageUrl = model.ImageUrl,
            IsPublic = model.IsPublic,
            Name = model.Name,
            Time = model.Time,
            VenueId = model.VenueId
        };

        var validationResult = ValidateEvent(@event, venue);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.EventRepository.Add(@event);

        ScheduleEventEmail(@event, chapter, settings);
        
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }
    
    public async Task DeleteEvent(Guid currentMemberId, Guid id)
    {
        var (@event, eventEmail, responses) = await _unitOfWork.RunAsync(
            x => x.EventRepository.GetById(id),
            x => x.EventEmailRepository.GetByEventId(id),
            x => x.EventResponseRepository.GetByEventId(id));
        
        AssertEventCanBeDeleted(eventEmail, responses);

        _unitOfWork.EventRepository.Delete(@event);
        await _unitOfWork.SaveChangesAsync();
    }
    
    public async Task<IReadOnlyCollection<EventInvitesDto>> GetChapterInvites(Guid currentMemberId, Guid chapterId,
        IReadOnlyCollection<Guid> eventIds)
    {
        if (eventIds.Count == 0)
        {
            return Array.Empty<EventInvitesDto>();
        }

        var (invites, emails) = await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
            x => x.EventInviteRepository.GetEventInvitesDtos(eventIds),
            x => x.EventEmailRepository.GetByEventIds(eventIds));

        return GetEventInvitesDtos(eventIds, invites, emails);
    }
    
    public async Task<IReadOnlyCollection<EventResponse>> GetChapterResponses(Guid currentMemberId,
        Guid chapterId, IReadOnlyCollection<Guid> eventIds)
    {
        if (eventIds.Count == 0)
        {
            return Array.Empty<EventResponse>();
        }

        return await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
            x => x.EventResponseRepository.GetByEventIds(eventIds));
    }

    public async Task<Event> GetEvent(Guid currentMemberId, Guid id)
    {
        var @event = await _unitOfWork.EventRepository.GetById(id).RunAsync();        
        await AssertMemberIsChapterAdmin(currentMemberId, @event.ChapterId);
        return @event;
    }
    
    public async Task<EventInvitesDto> GetEventInvites(Guid currentMemberId, Guid eventId)
    {
        var (@event, invites, eventEmail) = await _unitOfWork.RunAsync(
            x => x.EventRepository.GetById(eventId),
            x => x.EventInviteRepository.GetByEventId(eventId),
            x => x.EventEmailRepository.GetByEventId(eventId));

        await AssertMemberIsChapterAdmin(currentMemberId, @event.ChapterId);
        
        return new EventInvitesDto
        {
            EventId = eventId,
            Sent = invites.Count,
            SentUtc = eventEmail?.SentUtc,
            ScheduledUtc = eventEmail?.ScheduledUtc,
        };
    }

    public async Task<EventResponsesDto> GetEventResponsesDto(Guid currentMemberId, Guid eventId)
    {
        var (@event, responses) = await _unitOfWork.RunAsync(
            x => x.EventRepository.GetById(eventId),
            x => x.EventResponseRepository.GetByEventId(eventId));

        var members = await GetChapterAdminRestrictedContent(currentMemberId, @event.ChapterId,
            x => x.MemberRepository.GetByChapterId(@event.ChapterId));

        return new EventResponsesDto
        {
            Members = members,
            Responses = responses
        };
    }

    public async Task<IReadOnlyCollection<Event>> GetEvents(Guid currentMemberId, Guid chapterId, int page, int pageSize)
    {
        return await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
            x => x.EventRepository.GetByChapterId(chapterId, page, pageSize));
    }

    public async Task<EventsDto> GetEventsDto(Guid currentMemberId, Guid chapterId, int page, int pageSize)
    {
        var events = await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
            x => x.EventRepository.GetByChapterId(chapterId, page, pageSize));

        var eventIds = events
            .Select(x => x.Id)
            .ToArray();

        var venueIds = events
            .Select(x => x.VenueId)
            .Distinct()
            .ToArray();

        var (venues, invites, responses, emails) = await _unitOfWork.RunAsync(
            x => x.VenueRepository.GetByChapterId(chapterId, venueIds),
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

    public async Task<IReadOnlyCollection<Event>> GetEventsByVenue(Guid currentMemberId, Guid venueId)
    {
        var (venue, events) = await _unitOfWork.RunAsync(
            x => x.VenueRepository.GetById(venueId),
            x => x.EventRepository.GetByVenueId(venueId));

        await AssertMemberIsChapterAdmin(currentMemberId, venue.ChapterId);

        return events;
    }
    
    public async Task<DateTime?> GetNextAvailableEventDate(Guid currentMemberId, Chapter chapter)
    {
        var chapterId = chapter.Id;
        var startOfDay = chapter.CurrentTime.StartOfDay();

        var (events, settings) = await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
            x => x.EventRepository.GetByChapterId(chapterId, startOfDay),
            x => x.ChapterEventSettingsRepository.GetByChapterId(chapterId));

        if (settings.DefaultDayOfWeek == null)
        {
            return null;
        }

        var nextEventDate = startOfDay.Next(settings.DefaultDayOfWeek.Value);
        if (events.Count == 0)
        {
            return nextEventDate;
        }

        var eventDates = events
            .Select(x => x.Date)
            .ToArray();
        var lastEventDate = eventDates.Max();

        while (nextEventDate < lastEventDate)
        {
            if (!eventDates.Contains(nextEventDate))
            {
                return nextEventDate;
            }

            nextEventDate = nextEventDate.AddDays(7);
        }

        return nextEventDate;
    }

    public async Task SendEventInviteeEmail(Guid currentMemberId, Guid eventId, IEnumerable<EventResponseType> responseTypes,
        string subject, string body)
    {
        var (@event, responses, invites) = await _unitOfWork.RunAsync(
            x => x.EventRepository.GetById(eventId),
            x => x.EventResponseRepository.GetByEventId(eventId),
            x => x.EventInviteRepository.GetByEventId(eventId));        

        AssertEventEmailsCanBeSent(@event);

        responses = responses
            .Where(x => responseTypes.Contains(x.ResponseTypeId))
            .ToArray();

        var (chapterAdminMembers, currentMember, members, chapter) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(@event.ChapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberRepository.GetByChapterId(@event.ChapterId),
            x => x.ChapterRepository.GetById(@event.ChapterId));

        AssertMemberIsChapterAdmin(currentMember, chapter.Id, chapterAdminMembers);

        var responseDictionary = responses.ToDictionary(x => x.MemberId, x => x);

        if (responseTypes.Contains(EventResponseType.None))
        {
            foreach (var invite in invites.Where(x => !responseDictionary.ContainsKey(x.MemberId)))
            {
                var response = new EventResponse
                {
                    EventId = eventId,
                    MemberId = invite.MemberId,
                    ResponseTypeId = EventResponseType.None
                };

                responseDictionary.Add(invite.MemberId, response);
            }
        }

        var to = members
            .Where(x => x.EmailOptIn && responseDictionary.ContainsKey(x.Id))
            .ToArray();

        await _emailService.SendBulkEmail(currentMemberId, chapter, to, subject, body);
    }

    public async Task<ServiceResult> SendEventInvites(Guid currentMemberId, Guid eventId, bool test = false)
    {
        var (@event, eventEmail, responses, invites) = await _unitOfWork.RunAsync(
            x => x.EventRepository.GetById(eventId),
            x => x.EventEmailRepository.GetByEventId(eventId),
            x => x.EventResponseRepository.GetByEventId(eventId),
            x => x.EventInviteRepository.GetByEventId(eventId));

        var validationResult = ValidateEventEmailCanBeSent(@event);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        if (!test && eventEmail?.SentUtc != null)
        {
            return ServiceResult.Failure("Invites have already been sent for this event");
        }

        var (chapterAdminMembers, currentMember, chapter, venue, members) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(@event.ChapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.ChapterRepository.GetById(@event.ChapterId),
            x => x.VenueRepository.GetById(@event.VenueId),
            x => x.MemberRepository.GetByChapterId(@event.ChapterId));

        AssertMemberIsChapterAdmin(currentMember, @event.ChapterId, chapterAdminMembers);

        var chapterAdminMember = chapterAdminMembers.First(x => x.MemberId == currentMemberId);

        var parameters = GetEventEmailParameters(chapter, @event, venue);

        if (test)
        {
            var member = await _unitOfWork.MemberRepository.GetById(currentMemberId).RunAsync();
            await _emailService.SendEmail(chapter, member.GetEmailAddressee(), EmailType.EventInvite, parameters);
            return ServiceResult.Successful();
        }

        return await SendEventInvites(
            chapterAdminMember, 
            currentMember, 
            chapter,
            @event, 
            venue,
            responses,
            invites,
            members);
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
            var email = emailDictionary[@event.Id];
            var chapter = chapterDictionary[@event.ChapterId];

            if (@event.Date < chapter.CurrentTime.StartOfDay())
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

            var (venue, responses, invites, members) = await _unitOfWork.RunAsync(
                x => x.VenueRepository.GetById(@event.VenueId),
                x => x.EventResponseRepository.GetByEventId(@event.Id),
                x => x.EventInviteRepository.GetByEventId(@event.Id),
                x => x.MemberRepository.GetByChapterId(@event.ChapterId));

            try
            {
                await SendEventInvites(null, null, chapter, @event, venue, responses, invites, members);
            }            
            catch
            {
                // do nothing
            }
        }
    }

    public async Task<ServiceResult> UpdateEvent(Guid memberId, Guid id, CreateEvent model)
    {
        var (@event, venue) = await _unitOfWork.RunAsync(
            x => x.EventRepository.GetById(id),
            x => x.VenueRepository.GetById(model.VenueId));

        await AssertMemberIsChapterAdmin(memberId, @event.ChapterId);

        @event.Date = model.Date;
        @event.Description = model.Description;
        @event.ImageUrl = model.ImageUrl;
        @event.IsPublic = model.IsPublic;
        @event.Name = model.Name;
        @event.Time = model.Time;
        @event.VenueId = model.VenueId;

        var validationResult = ValidateEvent(@event, venue);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.EventRepository.Update(@event);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<EventResponse> UpdateMemberResponse(Guid currentMemberId, Guid eventId, Guid memberId, 
        EventResponseType responseType)
    {
        var (@event, response) = await _unitOfWork.RunAsync(
            x => x.EventRepository.GetById(eventId),
            x => x.EventResponseRepository.GetByMemberId(memberId, eventId));

        await AssertMemberIsChapterAdmin(currentMemberId, @event.ChapterId);

        if (response == null)
        {
            response = new EventResponse
            {
                EventId = eventId,
                MemberId = memberId,
                ResponseTypeId = responseType
            };
            _unitOfWork.EventResponseRepository.Add(response);
        }
        else
        {
            response.ResponseTypeId = responseType;
            _unitOfWork.EventResponseRepository.Update(response);
        }

        await _unitOfWork.SaveChangesAsync();

        return response;
    }

    public async Task<ServiceResult> UpdateScheduledEmail(Guid currentMemberId, Guid eventId, DateTime? date,
        string? time)
    {
        var (@event, eventEmail) = await _unitOfWork.RunAsync(
            x => x.EventRepository.GetById(eventId),
            x => x.EventEmailRepository.GetByEventId(eventId));

        await AssertMemberIsChapterAdmin(currentMemberId, @event.ChapterId);

        if (eventEmail == null && date == null && string.IsNullOrEmpty(time))
        {
            return ServiceResult.Successful();
        }

        if (!string.IsNullOrEmpty(time) && !TimeOnly.TryParse(time, out var parsedTime))
        {
            return ServiceResult.Failure("Invalid time");
        }

        if (date == null != string.IsNullOrEmpty(time))
        {
            return ServiceResult.Failure("Missing date or time");
        }

        if (eventEmail == null)
        {
            eventEmail = new EventEmail();
        }

        if (date < DateTime.Now)
        {
            return ServiceResult.Failure("Scheduled date cannot be in the past");
        }

        if (date > @event.Date)
        {
            return ServiceResult.Failure("Scheduled date cannot be after event");
        }

        eventEmail.ScheduledUtc = date != null
            ? date + TimeOnly.Parse(time ?? "").ToTimeSpan()
            : null;

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

    private ServiceResult ValidateEvent(Event @event, Venue venue)
    {
        if (string.IsNullOrWhiteSpace(@event.Name))
        {
            return ServiceResult.Failure("Name is required");
        }

        if (@event.Date == DateTime.MinValue)
        {
            return ServiceResult.Failure("Date is required");
        }

        if (venue.ChapterId != @event.ChapterId)
        {
            return ServiceResult.Failure("Venue not found");
        }
        
        return ServiceResult.Successful();
    }

    private static void AssertEventEmailsCanBeSent(Event @event)
    {
        ServiceResult result = ValidateEventEmailCanBeSent(@event);
        if (!result.Success)
        {
            throw new OdkServiceException(result.Message);
        }
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
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"chapter.name", chapter.Name},
            {"event.date", @event.Date.ToString("dddd dd MMMM, yyyy")},
            {"event.id", @event.Id.ToString()},
            {"event.location", venue.Name},
            {"event.name", @event.Name},
            {"event.time", @event.Time ?? ""}
        };

        parameters.Add("event.rsvpurl", (_settings.BaseUrl + _settings.EventRsvpUrlFormat).Interpolate(parameters));
        parameters.Add("event.url", (_settings.BaseUrl + _settings.EventUrlFormat).Interpolate(parameters));
        parameters.Add("unsubscribeUrl", (_settings.BaseUrl + _settings.UnsubscribeUrlFormat).Interpolate(parameters));

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
        if (settings?.DefaultScheduledEmailDayOfWeek == null ||
            string.IsNullOrEmpty(settings.DefaultScheduledEmailTimeOfDay))
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
        Member? currentMember,
        Chapter chapter,
        Event @event,
        Venue venue,
        IReadOnlyCollection<EventResponse> responses,
        IReadOnlyCollection<EventInvite> invites,
        IReadOnlyCollection<Member> members)
    {
        var parameters = GetEventEmailParameters(chapter, @event, venue);

        var memberResponses = responses.ToDictionary(x => x.MemberId, x => x);
        var inviteDictionary = invites.ToDictionary(x => x.MemberId, x => x);
        var invited = members
            .Where(x => x.EmailOptIn && !inviteDictionary.ContainsKey(x.Id) && !memberResponses.ContainsKey(x.Id))
            .ToArray();

        await _emailService.SendBulkEmail(
            chapterAdminMember,
            currentMember,
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
}
