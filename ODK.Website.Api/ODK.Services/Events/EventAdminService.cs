using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Mail;
using ODK.Core.Members;
using ODK.Core.Utils;
using ODK.Core.Venues;
using ODK.Services.Exceptions;
using ODK.Services.Mails;

namespace ODK.Services.Events
{
    public class EventAdminService : OdkAdminServiceBase, IEventAdminService
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IMailProviderFactory _mailProviderFactory;
        private readonly IMemberEmailRepository _memberEmailRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly EventAdminServiceSettings _settings;
        private readonly IVenueRepository _venueRepository;

        public EventAdminService(IEventRepository eventRepository, IChapterRepository chapterRepository,
            IMemberEmailRepository memberEmailRepository, EventAdminServiceSettings settings,
            IMemberRepository memberRepository, IVenueRepository venueRepository,
            IMailProviderFactory mailProviderFactory)
            : base(chapterRepository)
        {
            _chapterRepository = chapterRepository;
            _eventRepository = eventRepository;
            _mailProviderFactory = mailProviderFactory;
            _memberEmailRepository = memberEmailRepository;
            _memberRepository = memberRepository;
            _settings = settings;
            _venueRepository = venueRepository;
        }

        public async Task<Event> CreateEvent(Guid memberId, CreateEvent createEvent)
        {
            await AssertMemberIsChapterAdmin(memberId, createEvent.ChapterId);

            Member member = await _memberRepository.GetMember(memberId);

            Event @event = new Event(Guid.Empty, createEvent.ChapterId, $"{member.FirstName} {member.LastName}", createEvent.Name,
                createEvent.Date, createEvent.VenueId, createEvent.Time, null,
                createEvent.Description, createEvent.IsPublic, null);

            await ValidateEvent(@event);

            return await _eventRepository.CreateEvent(@event);
        }

        public async Task DeleteEvent(Guid currentMemberId, Guid id)
        {
            await AssertEventCanBeDeleted(currentMemberId, id);

            await _eventRepository.DeleteEvent(id);
        }

        public async Task<IReadOnlyCollection<EventInvites>> GetChapterInvites(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            IReadOnlyCollection<Event> events = await _eventRepository.GetEvents(chapterId, DateTime.UtcNow);

            IMailProvider mailProvider = await _mailProviderFactory.Create(chapterId);

            List<EventInvites> invites = new List<EventInvites>();
            foreach (Event @event in events)
            {
                EventInvites eventInvites = await mailProvider.GetEventInvites(@event);
                invites.Add(eventInvites);
            }

            return invites;
        }

        public async Task<IReadOnlyCollection<EventMemberResponse>> GetChapterResponses(Guid currentMemberId,
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

        public async Task<Email> GetEventEmail(Guid currentMemberId, Guid eventId)
        {
            Event @event = await GetEvent(currentMemberId, eventId);
            Chapter chapter = await _chapterRepository.GetChapter(@event.ChapterId);
            Venue venue = await _venueRepository.GetVenue(@event.VenueId);
            Email email = await GetEventEmail();
            IDictionary<string, string> parameters = GetEventEmailParameters(chapter, @event, venue);
            return email.Interpolate(parameters);
        }

        public async Task<EventInvites> GetEventInvites(Guid currentMemberId, Guid eventId)
        {
            Event @event = await GetEvent(currentMemberId, eventId);

            IMailProvider mailProvider = await _mailProviderFactory.Create(@event.ChapterId);
            return await mailProvider.GetEventInvites(@event);
        }

        public async Task<IReadOnlyCollection<EventMemberResponse>> GetEventResponses(Guid currentMemberId, Guid eventId)
        {
            Event @event = await GetEvent(currentMemberId, eventId);
            return await _eventRepository.GetEventResponses(@event.Id);
        }

        public async Task<IReadOnlyCollection<Event>> GetEvents(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _eventRepository.GetEvents(chapterId, null);
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

        public async Task SendEventInvites(Guid currentMemberId, Guid eventId)
        {
            Event @event = await GetEvent(currentMemberId, eventId);
            if (@event.Date < DateTime.UtcNow)
            {
                throw new OdkServiceException("Invites cannot be sent to past events");
            }

            IMailProvider mailProvider = await _mailProviderFactory.Create(@event.ChapterId);

            await mailProvider.SynchroniseMembers(@event.ChapterId);

            Chapter chapter = await _chapterRepository.GetChapter(@event.ChapterId);
            Venue venue = await _venueRepository.GetVenue(@event.VenueId);

            Email email = await GetEventEmail();
            IDictionary<string, string> parameters = GetEventEmailParameters(chapter, @event, venue);
            email = email.Interpolate(parameters);

            @event.EmailProviderEmailId = await mailProvider.SendEventEmail(@event, email);

            await _eventRepository.UpdateEvent(@event);
        }

        public async Task<Event> UpdateEvent(Guid memberId, Guid id, CreateEvent @event)
        {
            Event update = await GetEvent(memberId, id);

            update.Date = @event.Date;
            update.Description = @event.Description;
            update.ImageUrl = null;
            update.IsPublic = @event.IsPublic;
            update.Name = @event.Name;
            update.Time = @event.Time;
            update.VenueId = @event.VenueId;

            await ValidateEvent(update);

            await _eventRepository.UpdateEvent(update);

            return update;
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

        private async Task AssertEventCanBeDeleted(Guid currentMemberId, Guid id)
        {
            Event @event = await GetEvent(currentMemberId, id);

            if (!string.IsNullOrWhiteSpace(@event.EmailProviderEmailId))
            {
                throw new OdkServiceException("Events that have had invite emails sent cannot be deleted");
            }
        }

        private async Task<Email> GetEventEmail()
        {
            return await _memberEmailRepository.GetEmail(EmailType.EventInvite);
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

            return parameters;
        }
    }
}
