using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Cryptography;
using ODK.Core.Events;
using ODK.Core.Mail;
using ODK.Core.Members;
using ODK.Core.Utils;
using ODK.Services.Exceptions;
using ODK.Services.Mails;

namespace ODK.Services.Events
{
    public class EventAdminService : OdkAdminServiceBase, IEventAdminService
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IMailService _mailService;
        private readonly IMemberEmailRepository _memberEmailRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly EventAdminServiceSettings _settings;

        public EventAdminService(IEventRepository eventRepository, IChapterRepository chapterRepository,
            IMemberEmailRepository memberEmailRepository, EventAdminServiceSettings settings,
            IMemberRepository memberRepository,
            IMailService mailService)
            : base(chapterRepository)
        {
            _chapterRepository = chapterRepository;
            _eventRepository = eventRepository;
            _mailService = mailService;
            _memberEmailRepository = memberEmailRepository;
            _memberRepository = memberRepository;
            _settings = settings;
        }

        public async Task<Event> CreateEvent(Guid memberId, CreateEvent createEvent)
        {
            await AssertMemberIsChapterAdmin(memberId, createEvent.ChapterId);

            Member member = await _memberRepository.GetMember(memberId);

            Event @event = new Event(Guid.Empty, createEvent.ChapterId, $"{member.FirstName} {member.LastName}", createEvent.Name, createEvent.Date,
                createEvent.Location, createEvent.Time, null,
                createEvent.Address, createEvent.MapQuery, createEvent.Description, createEvent.IsPublic);

            ValidateEvent(@event);

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

            IReadOnlyCollection<MemberEventEmail> invites = await _memberEmailRepository.GetChapterEventEmails(chapterId);
            return MapEmailsToInvites(invites);
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
            Email email = await GetEventEmail();
            IDictionary<string, string> parameters = GetEventEmailParameters(chapter, @event);
            return email.Interpolate(parameters);
        }

        public async Task<EventInvites> GetEventInvites(Guid currentMemberId, Guid eventId)
        {
            Event @event = await GetEvent(currentMemberId, eventId);
            IReadOnlyCollection<MemberEventEmail> emails = await _memberEmailRepository.GetEventEmails(@event.Id);
            return MapEmailsToInvites(emails).SingleOrDefault();
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

        public async Task SendEventInvites(Guid currentMemberId, Guid eventId)
        {
            Event @event = await GetEvent(currentMemberId, eventId);
            if (@event.Date < DateTime.UtcNow)
            {
                throw new OdkServiceException("Invites cannot be sent to past events");
            }

            Chapter chapter = await _chapterRepository.GetChapter(@event.ChapterId);

            Email email = await GetEventEmail();
            IDictionary<string, string> parameters = GetEventEmailParameters(chapter, @event);
            email = email.Interpolate(parameters);

            IReadOnlyCollection<Member> members = await _memberRepository.GetMembers(@event.ChapterId);
            IReadOnlyCollection<MemberEventEmail> sent = await _memberEmailRepository.GetEventEmails(eventId);

            foreach (Member member in members.Where(member => sent.All(x => x.MemberId != member.Id)))
            {
                await SendEventInvite(@event, member, email);
            }
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

        private static IDictionary<string, string> GetMemberEmailParameters(string token)
        {
            IDictionary<string, string> parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(token))
            {
                parameters.Add("token", token);
            }

            return parameters;
        }

        private static IReadOnlyCollection<EventInvites> MapEmailsToInvites(IEnumerable<MemberEventEmail> emails)
        {
            return emails.GroupBy(x => x.EventId).Select(group => new EventInvites
            {
                Delivered = group.Count(x => x.Sent),
                EventId = group.Key,
                Sent = group.Count()
            }).ToArray();
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

        private async Task<Email> GetEventEmail()
        {
            return await _memberEmailRepository.GetEmail(EmailType.EventInvite);
        }

        private IDictionary<string, string> GetEventEmailParameters(Chapter chapter, Event @event)
        {
            IDictionary<string, string> parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"chapter.name", chapter.Name},
                {"event.date", @event.Date.ToString("dddd dd MMMM, yyyy")},
                {"event.id", @event.Id.ToString()},
                {"event.location", @event.Location},
                {"event.name", @event.Name},
                {"event.time", @event.Time}
            };

            parameters.Add("event.rsvpurl", (_settings.BaseUrl + _settings.EventRsvpUrlFormat).Interpolate(parameters));
            parameters.Add("event.url", (_settings.BaseUrl + _settings.EventUrlFormat).Interpolate(parameters));

            return parameters;
        }

        private async Task SendEventInvite(Event @event, Member member, Email email)
        {
            if (!member.EmailOptIn)
            {
                return;
            }

            string token = RandomStringGenerator.Generate(32);
            IDictionary<string, string> parameters = GetMemberEmailParameters(token);

            email = email.Interpolate(parameters);

            MemberEmail memberEmail = await _mailService.CreateMemberEmail(member, email, parameters);

            MemberEventEmail memberEventEmail = new MemberEventEmail(@event.Id, member.Id, memberEmail.Id, token, memberEmail.Sent);
            await _memberEmailRepository.AddMemberEventEmail(memberEventEmail);

            await _mailService.SendMemberMail(memberEmail, member, email);
        }
    }
}
