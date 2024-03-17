using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Utils;
using ODK.Core.Venues;
using ODK.Services.Emails;
using ODK.Services.Exceptions;

namespace ODK.Services.Events
{
    public class EventAdminService : OdkAdminServiceBase, IEventAdminService
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IEmailService _emailService;
        private readonly IEventRepository _eventRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly EventAdminServiceSettings _settings;
        private readonly IVenueRepository _venueRepository;

        public EventAdminService(IEventRepository eventRepository, IChapterRepository chapterRepository,
            EventAdminServiceSettings settings, IMemberRepository memberRepository, IVenueRepository venueRepository,
            IEmailService emailService)
            : base(chapterRepository)
        {
            _chapterRepository = chapterRepository;
            _emailService = emailService;
            _eventRepository = eventRepository;
            _memberRepository = memberRepository;
            _settings = settings;
            _venueRepository = venueRepository;
        }

        public async Task<ServiceResult> CreateEvent(Guid memberId, CreateEvent createEvent)
        {
            await AssertMemberIsChapterAdmin(memberId, createEvent.ChapterId);

            Member? member = await _memberRepository.GetMember(memberId);
            if (member == null)
            {
                throw new OdkServiceException("Invalid member id");
            }

            Event @event = new Event(Guid.Empty, createEvent.ChapterId, $"{member.FirstName} {member.LastName}", createEvent.Name,
                createEvent.Date, createEvent.VenueId, createEvent.Time, createEvent.ImageUrl,
                createEvent.Description, createEvent.IsPublic);

            ServiceResult validationResult = await ValidateEvent(@event);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            await _eventRepository.CreateEvent(@event);

            return ServiceResult.Successful();
        }
        
        public async Task DeleteEvent(Guid currentMemberId, Guid id)
        {
            await AssertEventCanBeDeleted(currentMemberId, id);

            await _eventRepository.DeleteEvent(id);
        }
        
        public async Task<IReadOnlyCollection<EventInvites>> GetChapterInvites(Guid currentMemberId, Guid chapterId,
            IReadOnlyCollection<Guid> eventIds)
        {
            if (eventIds.Count == 0)
            {
                return Array.Empty<EventInvites>();
            }


            Task<bool> authorisedTask = MemberIsChapterAdmin(currentMemberId, chapterId);
            Task<IReadOnlyCollection<EventInvite>> eventInvitesTask = _eventRepository.GetChapterInvites(chapterId, eventIds);
            Task<IReadOnlyCollection<EventEmail>> eventEmailsTask = _eventRepository.GetEventEmails(chapterId, eventIds);

            await Task.WhenAll(authorisedTask, eventInvitesTask, eventEmailsTask);

            if (!authorisedTask.Result)
            {
                throw new OdkNotAuthorizedException();
            }

            IDictionary<Guid, DateTime?> eventEmailDictionary = eventEmailsTask
                .Result
                .ToDictionary(x => x.EventId, x => x.SentDate);
            IDictionary<Guid, int> eventInvitesDictionary = eventInvitesTask
                .Result
                .GroupBy(x => x.EventId)
                .ToDictionary(x => x.Key, x => x.Count());

            return eventIds.Select(x => new EventInvites
            {
                EventId = x,
                Sent = eventInvitesDictionary.ContainsKey(x) ? eventInvitesDictionary[x] : 0,
                SentDate = eventEmailDictionary.ContainsKey(x) ? eventEmailDictionary[x] : default
            }).ToArray();
        }
        
        public async Task<IReadOnlyCollection<EventResponse>> GetChapterResponses(Guid currentMemberId,
            Guid chapterId, IReadOnlyCollection<Guid> eventIds)
        {
            if (eventIds.Count == 0)
            {
                return Array.Empty<EventResponse>();
            }

            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _eventRepository.GetChapterResponses(chapterId, eventIds);
        }

        public async Task<Event> GetEvent(Guid currentMemberId, Guid id)
        {
            Event? @event = await _eventRepository.GetEvent(id);
            if (@event == null)
            {
                throw new OdkNotFoundException();
            }

            await AssertMemberIsChapterAdmin(currentMemberId, @event.ChapterId);

            return @event;
        }
        
        public async Task<EventInvites> GetEventInvites(Guid currentMemberId, Guid eventId)
        {
            Event @event = await GetEvent(currentMemberId, eventId);
            EventEmail? eventEmail = await _eventRepository.GetEventEmail(@event.Id);
            IReadOnlyCollection<EventInvite> invites = await _eventRepository.GetEventInvites(@event.Id);
            return new EventInvites
            {
                EventId = eventId,
                Sent = invites.Count,
                SentDate = eventEmail?.SentDate
            };
        }

        public async Task<IReadOnlyCollection<EventResponse>> GetEventResponses(Guid currentMemberId, Guid eventId)
        {
            Event @event = await GetEvent(currentMemberId, eventId);
            IReadOnlyCollection<EventResponse> responses = await _eventRepository.GetEventResponses(@event.Id);
            IReadOnlyCollection<EventInvite> invited = await _eventRepository.GetEventInvites(@event.Id);
            IReadOnlyCollection<Member> members = await _memberRepository.GetMembers(@event.ChapterId);

            IDictionary<Guid, EventResponse> responseDictionary = responses.ToDictionary(x => x.MemberId, x => x);
            IDictionary<Guid, EventInvite> inviteDictionary = invited.ToDictionary(x => x.MemberId, x => x);

            return members.Select(member =>
            {
                EventResponseType responseType =
                    responseDictionary.ContainsKey(member.Id)
                    ? responseDictionary[member.Id].ResponseTypeId
                    : inviteDictionary.ContainsKey(member.Id)
                    ? EventResponseType.None
                    : EventResponseType.NotInvited;
                return new EventResponse(eventId, member.Id, responseType);
            }).ToArray();
        }

        public async Task<IReadOnlyCollection<Event>> GetEvents(Guid currentMemberId, Guid chapterId, int page, int pageSize)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _eventRepository.GetEvents(chapterId, page, pageSize);
        }

        public async Task<IReadOnlyCollection<Event>> GetEventsByVenue(Guid currentMemberId, Guid venueId)
        {
            Venue? venue = await _venueRepository.GetVenue(venueId);
            if (venue == null)
            {
                throw new OdkNotFoundException();
            }

            await AssertMemberIsChapterAdmin(currentMemberId, venue.ChapterId);

            return await _eventRepository.GetEventsByVenue(venueId);
        }
        
        public async Task SendEventInviteeEmail(Guid currentMemberId, Guid eventId, IEnumerable<EventResponseType> responseTypes,
            string subject, string body)
        {
            Event @event = await GetEvent(currentMemberId, eventId);

            AssertEventEmailsCanBeSent(@event);

            IReadOnlyCollection<Member> members = await _memberRepository.GetMembers(@event.ChapterId);

            IReadOnlyCollection<EventResponse> responses = await _eventRepository.GetEventResponses(@event.Id);
            responses = responses.Where(x => responseTypes.Contains(x.ResponseTypeId)).ToArray();

            IDictionary<Guid, EventResponse> responseDictionary = responses.ToDictionary(x => x.MemberId, x => x);

            if (responseTypes.Contains(EventResponseType.None))
            {
                IReadOnlyCollection<EventInvite> invites = await _eventRepository.GetEventInvites(eventId);
                foreach (EventInvite invite in invites.Where(x => !responseDictionary.ContainsKey(x.MemberId)))
                {
                    responseDictionary.Add(invite.MemberId, new EventResponse(eventId, invite.MemberId, EventResponseType.None));
                }
            }

            IReadOnlyCollection<Member> to = members
                .Where(x => x.EmailOptIn && responseDictionary.ContainsKey(x.Id))
                .ToArray();

            Chapter? chapter = await _chapterRepository.GetChapter(@event.ChapterId);

            await _emailService.SendBulkEmail(currentMemberId, chapter!, to, subject, body);
        }

        public async Task<ServiceResult> SendEventInvites(Guid currentMemberId, Guid eventId, bool test = false)
        {
            Event @event = await GetEvent(currentMemberId, eventId);

            ServiceResult validationResult = ValidateEventEmailCanBeSent(@event);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            EventEmail? eventEmail = await _eventRepository.GetEventEmail(@event.Id);
            if (!test && eventEmail?.SentDate != null)
            {
                return ServiceResult.Failure("Invites have already been sent for this event");
            }

            Venue? venue = await _venueRepository.GetVenue(@event.VenueId);
            Chapter? chapter = await _chapterRepository.GetChapter(@event.ChapterId);

            IDictionary<string, string?> parameters = GetEventEmailParameters(chapter!, @event, venue!);

            if (test)
            {
                Member? member = await _memberRepository.GetMember(currentMemberId);
                await _emailService.SendEmail(chapter!, member!.GetEmailAddressee(), EmailType.EventInvite, parameters);
            }
            else
            {
                IReadOnlyCollection<EventResponse> responses = await _eventRepository.GetEventResponses(@event.Id);
                IDictionary<Guid, EventResponse> memberResponses = responses.ToDictionary(x => x.MemberId, x => x);

                IReadOnlyCollection<EventInvite> invites = await _eventRepository.GetEventInvites(@event.Id);
                IDictionary<Guid, EventInvite> inviteDictionary = invites.ToDictionary(x => x.MemberId, x => x);

                IReadOnlyCollection<Member> members = await _memberRepository.GetMembers(chapter!.Id);
                IReadOnlyCollection<Member> invited = members
                    .Where(x => x.EmailOptIn && !inviteDictionary.ContainsKey(x.Id) && !memberResponses.ContainsKey(x.Id))
                    .ToArray();

                await _emailService.SendBulkEmail(currentMemberId, chapter, invited, EmailType.EventInvite, parameters);

                DateTime sentDate = DateTime.UtcNow;
                eventEmail = new EventEmail(Guid.Empty, @event.Id, sentDate);
                await _eventRepository.AddEventEmail(eventEmail);

                // Add null event responses to indicate that members have been invited
                await _eventRepository.AddEventInvites(@event.Id, invited.Select(x => x.Id), sentDate);
            }

            return ServiceResult.Successful();
        }

        public async Task<ServiceResult> UpdateEvent(Guid memberId, Guid id, CreateEvent @event)
        {
            Event update = await GetEvent(memberId, id);

            update.Date = @event.Date;
            update.Description = @event.Description;
            update.ImageUrl = @event.ImageUrl;
            update.IsPublic = @event.IsPublic;
            update.Name = @event.Name;
            update.Time = @event.Time;
            update.VenueId = @event.VenueId;

            ServiceResult validationResult = await ValidateEvent(update);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            await _eventRepository.UpdateEvent(update);

            return ServiceResult.Successful();
        }

        public async Task<Event> UpdateEventOld(Guid memberId, Guid id, CreateEvent @event)
        {
            Event update = await GetEvent(memberId, id);

            update.Date = @event.Date;
            update.Description = @event.Description;
            update.ImageUrl = @event.ImageUrl;
            update.IsPublic = @event.IsPublic;
            update.Name = @event.Name;
            update.Time = @event.Time;
            update.VenueId = @event.VenueId;

            await AssertValidEvent(update);

            await _eventRepository.UpdateEvent(update);

            return update;
        }

        public async Task<EventResponse> UpdateMemberResponse(Guid currentMemberId, Guid eventId, Guid memberId, EventResponseType responseType)
        {
            Event @event = await GetEvent(currentMemberId, eventId);

            EventResponse response = new EventResponse(@event.Id, memberId, responseType);

            await _eventRepository.UpdateEventResponse(response);

            return response;
        }

        private static ServiceResult ValidateEventEmailCanBeSent(Event @event)
        {
            if (@event.Date < DateTime.UtcNow.Date)
            {
                return ServiceResult.Failure("Invites cannot be sent to past events");
            }

            return ServiceResult.Successful();
        }

        private async Task<ServiceResult> ValidateEvent(Event @event)
        {
            if (string.IsNullOrWhiteSpace(@event.Name))
            {
                return ServiceResult.Failure("Name is required");
            }

            if (@event.Date == DateTime.MinValue)
            {
                return ServiceResult.Failure("Date is required");
            }

            Venue? venue = await _venueRepository.GetVenue(@event.VenueId);
            if (venue == null || venue.ChapterId != @event.ChapterId)
            {
                return ServiceResult.Failure("Venue not found");
            }
            
            return ServiceResult.Successful();
        }

        private async Task AssertValidEvent(Event @event)
        {
            ServiceResult result = await ValidateEvent(@event);
            if (!result.Success)
            {
                throw new OdkServiceException(result.Message);
            }
        }

        private static void AssertEventEmailsCanBeSent(Event @event)
        {
            ServiceResult result = ValidateEventEmailCanBeSent(@event);
            if (!result.Success)
            {
                throw new OdkServiceException(result.Message);
            }
        }

        private async Task AssertEventCanBeDeleted(Guid currentMemberId, Guid eventId)
        {
            EventEmail? eventEmail = await _eventRepository.GetEventEmail(eventId);
            if (eventEmail?.SentDate != null)
            {
                throw new OdkServiceException("Events that have had invite emails sent cannot be deleted");
            }

            IReadOnlyCollection<EventResponse> responses = await _eventRepository.GetEventResponses(eventId);
            if (responses.Count > 0)
            {
                throw new OdkServiceException("Events with responses cannot be deleted");
            }

            Event? @event = await _eventRepository.GetEvent(eventId);
            if (@event != null)
            {
                await AssertMemberIsChapterAdmin(currentMemberId, @event.ChapterId);
            }
        }

        private IDictionary<string, string?> GetEventEmailParameters(Chapter chapter, Event @event, Venue venue)
        {
            IDictionary<string, string?> parameters = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                {"chapter.name", chapter.Name},
                {"event.date", @event.Date.ToString("dddd dd MMMM, yyyy")},
                {"event.id", @event.Id.ToString()},
                {"event.location", venue.Name},
                {"event.name", @event.Name},
                {"event.time", @event.Time}
            };

            parameters.Add("event.rsvpurl", (_settings.BaseUrl + _settings.EventRsvpUrlFormat).Interpolate(parameters));
            parameters.Add("event.url", (_settings.BaseUrl + _settings.EventUrlFormat).Interpolate(parameters));
            parameters.Add("unsubscribeUrl", (_settings.BaseUrl + _settings.UnsubscribeUrlFormat).Interpolate(parameters));

            return parameters;
        }
    }
}
