using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Emails;
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

        public async Task<Event> CreateEvent(Guid memberId, CreateEvent createEvent)
        {
            await AssertMemberIsChapterAdmin(memberId, createEvent.ChapterId);

            Member member = await _memberRepository.GetMember(memberId);

            Event @event = new Event(Guid.Empty, createEvent.ChapterId, $"{member.FirstName} {member.LastName}", createEvent.Name,
                createEvent.Date, createEvent.VenueId, createEvent.Time, createEvent.ImageUrl,
                createEvent.Description, createEvent.IsPublic);

            await ValidateEvent(@event);

            return await _eventRepository.CreateEvent(@event);
        }

        public async Task DeleteEvent(Guid currentMemberId, Guid id)
        {
            await AssertEventCanBeDeleted(currentMemberId, id);

            await _eventRepository.DeleteEvent(id);
        }

        public async Task<IReadOnlyCollection<EventInvites>> GetChapterInvites(Guid currentMemberId, Guid chapterId, int page, int pageSize)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            IReadOnlyCollection<Event> events = await _eventRepository.GetEvents(chapterId, page, pageSize);
            IReadOnlyCollection<EventEmail> eventEmails = events.Count > 0
                ? await _eventRepository.GetEventEmails(chapterId, events.Min(x => x.Date))
                : new EventEmail[0];
            IReadOnlyCollection<EventInvite> eventInvites = events.Count > 0
                ? await _eventRepository.GetEventInvites(chapterId, events.Min(x => x.Date))
                : new EventInvite[0];

            IDictionary<Guid, DateTime?> eventEmailDictionary = eventEmails.ToDictionary(x => x.EventId, x => x.SentDate);
            IDictionary<Guid, int> eventInvitesDictionary = eventInvites
                .GroupBy(x => x.EventId)
                .ToDictionary(x => x.Key, x => x.Count());

            return events.Select(x => new EventInvites
            {
                EventId = x.Id,
                Sent = eventInvitesDictionary.ContainsKey(x.Id) ? eventInvitesDictionary[x.Id] : 0,
                SentDate = eventEmailDictionary.ContainsKey(x.Id) ? eventEmailDictionary[x.Id] : default
            }).ToArray();
        }

        public async Task<IReadOnlyCollection<EventResponse>> GetChapterResponses(Guid currentMemberId,
            Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _eventRepository.GetChapterResponses(chapterId);
        }

        public async Task<Event> GetEvent(Guid currentMemberId, Guid id)
        {
            Event @event = await _eventRepository.GetEvent(id);
            if (@event == null)
            {
                throw new OdkNotFoundException();
            }

            await AssertMemberIsChapterAdmin(currentMemberId, @event.ChapterId);

            return @event;
        }

        public async Task<int> GetEventCount(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _eventRepository.GetEventCount(chapterId);
        }

        public async Task<EventInvites> GetEventInvites(Guid currentMemberId, Guid eventId)
        {
            Event @event = await GetEvent(currentMemberId, eventId);
            EventEmail eventEmail = await _eventRepository.GetEventEmail(@event.Id);
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
            Venue venue = await _venueRepository.GetVenue(venueId);
            if (venue == null)
            {
                throw new OdkNotFoundException();
            }

            await AssertMemberIsChapterAdmin(currentMemberId, venue.ChapterId);

            return await _eventRepository.GetEventsByVenue(venueId);
        }

        public async Task<IReadOnlyCollection<EventResponse>> GetMemberResponses(Guid currentMemberId, Guid memberId)
        {
            Member member = await _memberRepository.GetMember(memberId);
            if (member == null)
            {
                throw new OdkNotFoundException();
            }

            await AssertMemberIsChapterAdmin(currentMemberId, member.ChapterId);

            return await _eventRepository.GetMemberResponses(memberId, true);
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

            Chapter chapter = await _chapterRepository.GetChapter(@event.ChapterId);

            await _emailService.SendBulkEmail(currentMemberId, chapter, to, subject, body);
        }

        public async Task SendEventInvites(Guid currentMemberId, Guid eventId, bool test = false)
        {
            Event @event = await GetEvent(currentMemberId, eventId);

            AssertEventEmailsCanBeSent(@event);

            EventEmail eventEmail = await _eventRepository.GetEventEmail(@event.Id);
            if (!test && eventEmail?.SentDate != null)
            {
                throw new OdkServiceException("Invites have already been sent for this event");
            }

            Venue venue = await _venueRepository.GetVenue(@event.VenueId);
            Chapter chapter = await _chapterRepository.GetChapter(@event.ChapterId);

            IDictionary<string, string> parameters = GetEventEmailParameters(chapter, @event, venue);

            if (test)
            {
                Member member = await _memberRepository.GetMember(currentMemberId);
                await _emailService.SendEmail(chapter, member.EmailAddress, EmailType.EventInvite, parameters);
            }
            else
            {
                IReadOnlyCollection<EventInvite> invites = await _eventRepository.GetEventInvites(@event.Id);
                IDictionary<Guid, EventInvite> inviteDictionary = invites.ToDictionary(x => x.MemberId, x => x);

                IReadOnlyCollection<Member> members = await _memberRepository.GetMembers(chapter.Id);
                IReadOnlyCollection<Member> invited = members
                    .Where(x => x.EmailOptIn && !inviteDictionary.ContainsKey(x.Id))
                    .ToArray();

                await _emailService.SendBulkEmail(currentMemberId, chapter, invited, EmailType.EventInvite, parameters);

                DateTime sentDate = DateTime.UtcNow;
                eventEmail = new EventEmail(Guid.Empty, @event.Id, sentDate);
                await _eventRepository.AddEventEmail(eventEmail);

                // Add null event responses to indicate that members have been invited
                await _eventRepository.AddEventInvites(@event.Id, invited.Select(x => x.Id), sentDate);
            }
        }

        public async Task<Event> UpdateEvent(Guid memberId, Guid id, CreateEvent @event)
        {
            Event update = await GetEvent(memberId, id);

            update.Date = @event.Date;
            update.Description = @event.Description;
            update.ImageUrl = @event.ImageUrl;
            update.IsPublic = @event.IsPublic;
            update.Name = @event.Name;
            update.Time = @event.Time;
            update.VenueId = @event.VenueId;

            await ValidateEvent(update);

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

        private async Task ValidateEvent(Event @event)
        {
            Venue venue = await _venueRepository.GetVenue(@event.VenueId);

            if (string.IsNullOrWhiteSpace(@event.Name) ||
                venue == null ||
                @event.Date == DateTime.MinValue)
            {
                throw new OdkServiceException("Some required fields are missing");
            }

            if (venue.ChapterId != @event.ChapterId)
            {
                throw new OdkServiceException("Events cannot be created for venues from other chapters");
            }
        }

        private static void AssertEventEmailsCanBeSent(Event @event)
        {
            if (@event.Date < DateTime.UtcNow)
            {
                throw new OdkServiceException("Invites cannot be sent to past events");
            }
        }

        private async Task AssertEventCanBeDeleted(Guid currentMemberId, Guid eventId)
        {
            EventEmail eventEmail = await _eventRepository.GetEventEmail(eventId);
            if (eventEmail?.SentDate != null)
            {
                throw new OdkServiceException("Events that have had invite emails sent cannot be deleted");
            }

            IReadOnlyCollection<EventResponse> responses = await _eventRepository.GetEventResponses(eventId);
            if (responses.Count > 0)
            {
                throw new OdkServiceException("Events with responses cannot be deleted");
            }

            Event @event = await _eventRepository.GetEvent(eventId);
            if (@event != null)
            {
                await AssertMemberIsChapterAdmin(currentMemberId, @event.ChapterId);
            }
        }

        private IDictionary<string, string> GetEventEmailParameters(Chapter chapter, Event @event, Venue venue)
        {
            IDictionary<string, string> parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
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

        private void ValidateEventResponse(EventResponse response)
        {
            if (!Enum.IsDefined(typeof(EventResponseType), response.ResponseTypeId) || response.ResponseTypeId <= EventResponseType.None)
            {
                throw new OdkServiceException("Invalid response type");
            }
        }
    }
}
