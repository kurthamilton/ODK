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
        private readonly IEventRepository _eventRepository;
        private readonly IMailProviderFactory _mailProviderFactory;
        private readonly IEmailRepository _emailRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly EventAdminServiceSettings _settings;
        private readonly IVenueRepository _venueRepository;

        public EventAdminService(IEventRepository eventRepository, IChapterRepository chapterRepository,
            IEmailRepository memberEmailRepository, EventAdminServiceSettings settings,
            IMemberRepository memberRepository, IVenueRepository venueRepository,
            IMailProviderFactory mailProviderFactory, IEmailService emailService)
            : base(chapterRepository)
        {
            _chapterRepository = chapterRepository;
            _eventRepository = eventRepository;
            _mailProviderFactory = mailProviderFactory;
            _emailRepository = memberEmailRepository;
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

            IMailProvider mailProvider = await _mailProviderFactory.Create(chapterId);

            return await mailProvider.GetInvites(chapterId, eventEmails);
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

        public async Task<int> GetEventCount(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _eventRepository.GetEventCount(chapterId);
        }

        public async Task<Email> GetEventEmail(Guid currentMemberId, Guid eventId)
        {
            Event @event = await GetEvent(currentMemberId, eventId);
            Chapter chapter = await _chapterRepository.GetChapter(@event.ChapterId);
            Venue venue = await _venueRepository.GetVenue(@event.VenueId);
            Email email = await GetEventEmailTemplate(chapter.Id);
            IDictionary<string, string> parameters = GetEventEmailParameters(chapter, @event, venue);
            return email.Interpolate(parameters);
        }

        public async Task<EventInvites> GetEventInvites(Guid currentMemberId, Guid eventId)
        {
            Event @event = await GetEvent(currentMemberId, eventId);
            EventEmail eventEmail = await _eventRepository.GetEventEmail(eventId);

            IMailProvider mailProvider = await _mailProviderFactory.Create(@event.ChapterId);
            return await mailProvider.GetEventInvites(@event, eventEmail);
        }

        public async Task<IReadOnlyCollection<EventMemberResponse>> GetEventResponses(Guid currentMemberId, Guid eventId)
        {
            Event @event = await GetEvent(currentMemberId, eventId);
            return await _eventRepository.GetEventResponses(@event.Id);
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

        public async Task<IReadOnlyCollection<EventMemberResponse>> GetMemberResponses(Guid currentMemberId, Guid memberId)
        {
            Member member = await _memberRepository.GetMember(memberId);
            if (member == null)
            {
                throw new OdkNotFoundException();
            }

            await AssertMemberIsChapterAdmin(currentMemberId, member.ChapterId);

            return await _eventRepository.GetMemberResponses(memberId, true);
        }

        public async Task SendEventInvites(Guid currentMemberId, Guid eventId, bool test = false)
        {
            Event @event = await GetEvent(currentMemberId, eventId);
            if (@event.Date < DateTime.UtcNow)
            {
                throw new OdkServiceException("Invites cannot be sent to past events");
            }

            EventEmail eventEmail = await _eventRepository.GetEventEmail(@event.Id);
            if (!test && eventEmail?.SentDate != null)
            {
                throw new OdkServiceException("Invites have already been sent for this event");
            }

            Chapter chapter = await _chapterRepository.GetChapter(@event.ChapterId);

            IMailProvider mailProvider = await _mailProviderFactory.Create(chapter);

            await mailProvider.SynchroniseMembers();

            if (eventEmail == null)
            {
                eventEmail = await CreateEventEmail(mailProvider, @event, chapter);
            }
            else
            {
                await UpdateEventEmail(mailProvider, @event, chapter, eventEmail);
            }

            if (test)
            {
                Member member = await _memberRepository.GetMember(currentMemberId);
                await mailProvider.SendTestEventEmail(eventEmail.EmailProviderEmailId, member);
            }
            else
            {
                IReadOnlyCollection<Member> invited = await mailProvider.SendEventEmail(@event, eventEmail);

                eventEmail.SentDate = DateTime.UtcNow;
                await _eventRepository.UpdateEventEmail(eventEmail);

                // Add null event responses to indicate that members have been invited
                await _eventRepository.AddEventInvites(@event.Id, invited.Select(x => x.Id));
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

        private async Task AssertEventCanBeDeleted(Guid currentMemberId, Guid eventId)
        {
            EventEmail eventEmail = await _eventRepository.GetEventEmail(eventId);
            if (eventEmail?.SentDate != null)
            {
                throw new OdkServiceException("Events that have had invite emails sent cannot be deleted");
            }

            IReadOnlyCollection<EventMemberResponse> responses = await _eventRepository.GetEventResponses(eventId);
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

        private async Task<EventEmail> CreateEventEmail(IMailProvider mailProvider, Event @event, Chapter chapter)
        {
            Email email = await GetEventEmail(mailProvider, @event, chapter);

            string emailProviderEmailId = await mailProvider.CreateEventEmail(@event, email);

            EventEmail eventEmail = new EventEmail(Guid.Empty, @event.Id, mailProvider.Name, emailProviderEmailId, null);

            Guid id = await _eventRepository.AddEventEmail(eventEmail);

            return new EventEmail(id, @event.Id, mailProvider.Name, emailProviderEmailId, null);
        }

        private async Task<Email> GetEventEmail(IMailProvider mailProvider, Event @event, Chapter chapter)
        {
            Venue venue = await _venueRepository.GetVenue(@event.VenueId);

            Email template = await GetEventEmailTemplate(chapter.Id);
            IDictionary<string, string> parameters = GetEventEmailParameters(chapter, @event, venue);
            Email email = template.Interpolate(parameters);

            return await mailProvider.GetEmailWithLayout(chapter, email);
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

        private async Task<Email> GetEventEmailTemplate(Guid chapterId)
        {
            return await _emailRepository.GetEmail(EmailType.EventInvite, chapterId);
        }

        private async Task UpdateEventEmail(IMailProvider mailProvider, Event @event, Chapter chapter, EventEmail eventEmail)
        {
            Email email = await GetEventEmail(mailProvider, @event, chapter);

            await mailProvider.UpdateEventEmail(@event, email, eventEmail.EmailProviderEmailId);
        }
    }
}
