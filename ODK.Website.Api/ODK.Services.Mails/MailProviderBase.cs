using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Mail;
using ODK.Core.Members;
using ODK.Services.Events;
using ODK.Services.Exceptions;

namespace ODK.Services.Mails
{
    public abstract class MailProviderBase : IMailProvider
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IMemberRepository _memberRepository;

        protected MailProviderBase(IChapterRepository chapterRepository, IMemberRepository memberRepository)
        {
            _chapterRepository = chapterRepository;
            _memberRepository = memberRepository;
        }

        public async Task<string> CreateEventEmail(ChapterEmailSettings emailSettings, Event @event, Chapter chapter, Email email)
        {
            ContactList contactList = await GetEventContactList(emailSettings, chapter);

            EventCampaign campaign = new EventCampaign
            {
                ContactListId = contactList.Id,
                From = emailSettings.FromEmailAddress,
                FromName = emailSettings.FromEmailName,
                HtmlContent = email.Body,
                Name = $"Event: {@event.Name}",
                Subject = email.Subject
            };

            campaign.Id = await CreateCampaign(emailSettings.EmailApiKey, campaign);

            await UpdateCampaignEmailContent(emailSettings.EmailApiKey, campaign);

            return campaign.Id;
        }

        public async Task<EventInvites> GetEventInvites(Event @event, EventEmail eventEmail)
        {
            ChapterEmailSettings emailSettings = await _chapterRepository.GetChapterEmailSettings(@event.ChapterId);
            EventCampaignStats stats = await GetEventStats(emailSettings.EmailApiKey, eventEmail);
            return new EventInvites
            {
                EventId = @event.Id,
                Sent = stats.Sent,
                SentDate = eventEmail.SentDate
            };
        }

        public async Task<IReadOnlyCollection<EventInvites>> GetInvites(Guid chapterId, IEnumerable<EventEmail> eventEmails)
        {
            ChapterEmailSettings emailSettings = await _chapterRepository.GetChapterEmailSettings(chapterId);
            IReadOnlyCollection<EventCampaignStats> stats = await GetStats(emailSettings.EmailApiKey, eventEmails);
            IDictionary<Guid, EventCampaignStats> statsDictionary = stats.ToDictionary(x => x.EventId, x => x);
            return eventEmails.Select(x => new EventInvites
            {
                EventId = x.EventId,
                Sent = statsDictionary.ContainsKey(x.EventId) ? statsDictionary[x.EventId].Sent : 0,
                SentDate = x.SentDate
            }).ToArray();
        }

        public async Task<bool> GetMemberOptIn(Member member)
        {
            ChapterEmailSettings emailSettings = await _chapterRepository.GetChapterEmailSettings(member.ChapterId);
            Contact contact = await GetContact(emailSettings.EmailApiKey, member.EmailAddress);
            return contact.OptIn;
        }

        public async Task SendEventEmail(ChapterEmailSettings emailSettings, string id)
        {
            await SendCampaignEmail(emailSettings.EmailApiKey, id);
        }

        public async Task SendTestEventEmail(ChapterEmailSettings emailSettings, string id, Member member)
        {
            await SendTestCampaignEmail(emailSettings.EmailApiKey, id, member.EmailAddress);
        }

        public async Task SynchroniseMembers(ChapterEmailSettings emailSettings, Chapter chapter)
        {
            ContactList contactList = await GetEventContactList(emailSettings, chapter);

            IReadOnlyCollection<Member> members = await _memberRepository.GetMembers(emailSettings.ChapterId);
            IReadOnlyCollection<Contact> contacts = await GetContacts(emailSettings.EmailApiKey, contactList.Id);
            IReadOnlyCollection<(Contact Contact, Member Member)> map = GetContactMap(contacts, members).ToArray();

            // add new members
            IEnumerable<Member> newMembers = map
                .Where(x => x.Contact == null)
                .Select(x => x.Member);
            foreach (Member member in newMembers)
            {
                await CreateContact(emailSettings.EmailApiKey, member, contactList);
            }

            // opt-out members who have unsubscribed
            IEnumerable<Member> unsubscribeMembers = map
                .Where(x => x.Member?.EmailOptIn == true && x.Contact?.OptIn == false)
                .Select(x => x.Member);
            foreach (Member member in unsubscribeMembers)
            {
                await _memberRepository.UpdateMember(member.Id, false, member.FirstName, member.LastName);
            }

            // delete old members
            IEnumerable<Contact> deleteContacts = map
                .Where(x => x.Member == null)
                .Select(x => x.Contact);
            foreach (Contact contact in deleteContacts)
            {
                await DeleteContact(emailSettings.EmailApiKey, contact);
            }
        }

        public async Task UpdateEventEmail(ChapterEmailSettings emailSettings, Event @event, Chapter chapter, Email email,
            string emailId)
        {
            ContactList contactList = await GetEventContactList(emailSettings, chapter);

            EventCampaign campaign = new EventCampaign
            {
                ContactListId = contactList.Id,
                From = emailSettings.FromEmailAddress,
                FromName = emailSettings.FromEmailName,
                HtmlContent = email.Body,
                Id = emailId,
                Name = $"Event: {@event.Name}",
                Subject = email.Subject
            };

            await UpdateCampaign(emailSettings.EmailApiKey, campaign);
        }

        public async Task UpdateMemberOptIn(Member member, bool optIn)
        {
            ChapterEmailSettings emailSettings = await _chapterRepository.GetChapterEmailSettings(member.ChapterId);

            await UpdateContactOptIn(emailSettings.EmailApiKey, member.EmailAddress, optIn);
        }

        protected abstract Task<string> CreateCampaign(string apiKey, EventCampaign campaign);

        protected abstract Task CreateContact(string apiKey, Member member, ContactList contactList);

        protected abstract Task DeleteContact(string apiKey, Contact contact);

        protected abstract Task<Contact> GetContact(string apiKey, string emailAddress);

        protected abstract Task<ContactList> GetContactList(string apiKey, string name);

        protected abstract Task<IReadOnlyCollection<Contact>> GetContacts(string apiKey, string contactListId);

        protected abstract Task<EventCampaignStats> GetEventStats(string apiKey, EventEmail eventEmail);

        protected abstract Task<IReadOnlyCollection<EventCampaignStats>> GetStats(string apiKey, IEnumerable<EventEmail> eventEmails);

        protected abstract Task SendCampaignEmail(string apiKey, string id);

        protected abstract Task SendTestCampaignEmail(string apiKey, string id, string to);

        protected abstract Task UpdateCampaign(string apiKey, EventCampaign campaign);

        protected abstract Task UpdateCampaignEmailContent(string apiKey, EventCampaign campaign);

        protected abstract Task UpdateContactOptIn(string apiKey, string emailAddress, bool optIn);

        private IEnumerable<(Contact, Member)> GetContactMap(IReadOnlyCollection<Contact> contacts,
            IReadOnlyCollection<Member> members)
        {
            IDictionary<string, Contact> contactDictionary = new Dictionary<string, Contact>(
                contacts.ToDictionary(x => x.EmailAddress, x => x),
                StringComparer.OrdinalIgnoreCase);
            IDictionary<string, Member> memberDictionary = new Dictionary<string, Member>(
                members.ToDictionary(x => x.EmailAddress, x => x),
                StringComparer.OrdinalIgnoreCase);

            foreach (Contact contact in contacts)
            {
                Member member = memberDictionary.ContainsKey(contact.EmailAddress) ? memberDictionary[contact.EmailAddress] : null;
                yield return (contact, member);
            }

            foreach (Member member in members.Where(x => !contactDictionary.ContainsKey(x.EmailAddress)))
            {
                yield return (null, member);
            }
        }

        private async Task<ContactList> GetEventContactList(ChapterEmailSettings emailSettings, Chapter chapter)
        {
            string listName = $"{chapter.Name} Events";

            ContactList contactList = await GetContactList(emailSettings.EmailApiKey, listName);
            if (contactList == null)
            {
                throw new OdkServiceException($"Contact list not found: {listName}");
            }

            return contactList;
        }
    }
}
