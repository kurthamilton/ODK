using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Mail;
using ODK.Core.Utils;
using ODK.Services.Exceptions;

namespace ODK.Services.Events
{
    public class EventAdminService : OdkAdminServiceBase, IEventAdminService
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IMemberEmailRepository _memberEmailRepository;
        private readonly EventAdminServiceSettings _settings;

        public EventAdminService(IEventRepository eventRepository, IChapterRepository chapterRepository,
            IMemberEmailRepository memberEmailRepository, EventAdminServiceSettings settings)
            : base(chapterRepository)
        {
            _chapterRepository = chapterRepository;
            _eventRepository = eventRepository;
            _memberEmailRepository = memberEmailRepository;
            _settings = settings;
        }

        public async Task<Event> CreateEvent(Guid memberId, CreateEvent createEvent)
        {
            await AssertMemberIsChapterAdmin(memberId, createEvent.ChapterId);

            Event @event = new Event(Guid.Empty, createEvent.ChapterId, createEvent.Name, createEvent.Date, createEvent.Location, createEvent.Time, null,
                createEvent.Address, createEvent.MapQuery, createEvent.Description, createEvent.IsPublic);

            ValidateEvent(@event);

            return await _eventRepository.CreateEvent(@event);
        }

        public async Task DeleteEvent(Guid currentMemberId, Guid id)
        {
            await AssertEventCanBeDeleted(currentMemberId, id);

            await _eventRepository.DeleteEvent(id);
        }

        public async Task<IReadOnlyCollection<EventMemberResponse>> GetChapterResponses(Guid currentMemberId, Guid chapterId)
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
            Email email = await _memberEmailRepository.GetEmail(EmailType.EventInvite);

            string subject = ReplaceEventEmailTokens(email.Subject, chapter, @event);
            string body = ReplaceEventEmailTokens(email.Body, chapter, @event);

            return new Email(EmailType.EventInvite, subject, body);
        }

        public async Task<IReadOnlyCollection<Event>> GetEvents(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _eventRepository.GetEvents(chapterId, null);
        }

        public async Task<IReadOnlyCollection<EventInvites>> GetMemberEventEmails(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            IReadOnlyCollection<MemberEventEmail> invites = await _memberEmailRepository.GetMemberEventEmails(chapterId);
            return invites.GroupBy(x => x.EventId).Select(group => new EventInvites
            {
                Delivered = group.Count(x => x.Sent),
                EventId = group.Key,
                Sent = group.Count()
            }).ToArray();
        }

        public async Task<Event> UpdateEvent(Guid memberId, Guid id, CreateEvent @event)
        {
            Event update = await GetEvent(memberId, id);

            update.Update(@event.Address, @event.Date, @event.Description, null, @event.IsPublic, @event.Location,
                @event.MapQuery, @event.Name, @event.Time);

            ValidateEvent(update);

            await _eventRepository.UpdateEvent(update);

            return update;
        }

        private static void ValidateEvent(Event @event)
        {
            if (string.IsNullOrWhiteSpace(@event.Name) ||
                string.IsNullOrWhiteSpace(@event.Location) ||
                @event.Date == DateTime.MinValue)
            {
                throw new OdkServiceException("Some required fields are missing");
            }
        }

        private async Task AssertEventCanBeDeleted(Guid currentMemberId, Guid id)
        {
            Event @event = await GetEvent(currentMemberId, id);

            IReadOnlyCollection<EventMemberResponse> responses = await _eventRepository.GetEventResponses(@event.Id);
            if (responses.Count > 0)
            {
                throw new OdkServiceException("Events with responses cannot be deleted");
            }
        }

        private string ReplaceEventEmailTokens(string text, Chapter chapter, Event @event, MemberEventEmail eventEmail = null)
        {
            IDictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "chapter.name", chapter.Name },
                { "event.date", @event.Date.ToString("dddd dd MMMM, yyyy") },
                { "event.id", @event.Id.ToString() },
                { "event.location", @event.Location },
                { "event.name", @event.Name },
                { "event.time", @event.Time }
            };

            if (text.Contains("{event.rsvpurl}"))
            {
                values.Add("event.rsvpurl", ReplaceEventEmailTokens(_settings.BaseUrl + _settings.EventRsvpUrlFormat, chapter, @event, eventEmail));
            }

            if (text.Contains("{event.url}"))
            {
                values.Add("event.url", ReplaceEventEmailTokens(_settings.BaseUrl + _settings.EventUrlFormat, chapter, @event, eventEmail));
            }

            if (eventEmail != null)
            {
                values.Add("token", eventEmail.ResponseToken);
            }

            return text.Interpolate(values);
        }
    }
}
