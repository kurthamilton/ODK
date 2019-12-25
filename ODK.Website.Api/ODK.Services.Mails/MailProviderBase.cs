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

        protected MailProviderBase(ChapterEmailProviderSettings settings, Chapter chapter, IChapterRepository chapterRepository,
            IMemberRepository memberRepository)
        {
            _chapterRepository = chapterRepository;
            _memberRepository = memberRepository;
            Chapter = chapter;
            Settings = settings;
        }

        public abstract string Name { get; }

        protected Chapter Chapter { get; }

        protected ChapterEmailProviderSettings Settings { get; }

        public async Task<string> CreateEventEmail(Event @event, Email email)
        {
            ContactList contactList = await GetEventContactList();

            EventCampaign campaign = new EventCampaign
            {
                ContactListId = contactList.Id,
                From = Settings.FromEmailAddress,
                FromName = Settings.FromName,
                HtmlContent = email.Body,
                Name = $"Event: {@event.Name}",
                Subject = email.Subject
            };

            campaign.Id = await CreateCampaign(campaign);

            await UpdateCampaignEmailContent(campaign);

            return campaign.Id;
        }

        public async Task<EventInvites> GetEventInvites(Event @event, EventEmail eventEmail)
        {
            EventCampaignStats stats = await GetEventStats(eventEmail);
            return new EventInvites
            {
                EventId = @event.Id,
                Sent = stats.Sent,
                SentDate = eventEmail?.SentDate
            };
        }

        public async Task<IReadOnlyCollection<EventInvites>> GetInvites(Guid chapterId, IEnumerable<EventEmail> eventEmails)
        {
            ChapterEmailSettings emailSettings = await _chapterRepository.GetChapterEmailSettings(chapterId);
            IReadOnlyCollection<EventCampaignStats> stats = await GetStats(eventEmails);
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
            Contact contact = await GetContact(member.EmailAddress);
            return contact.OptIn;
        }

        public async Task SendEventEmail(string id)
        {
            await SendCampaignEmail(id);
        }

        public async Task SendTestEventEmail(string id, Member member)
        {
            await SendTestCampaignEmail(id, member.EmailAddress);
        }

        public async Task SynchroniseMembers()
        {
            ContactList contactList = await GetEventContactList();

            IReadOnlyCollection<Member> members = await _memberRepository.GetMembers(Chapter.Id);
            IReadOnlyCollection<Contact> contacts = await GetContacts(contactList.Id);
            IReadOnlyCollection<(Contact Contact, Member Member)> map = GetContactMap(contacts, members).ToArray();

            // add new members
            IEnumerable<Member> newMembers = map
                .Where(x => x.Contact == null)
                .Select(x => x.Member);
            foreach (Member member in newMembers)
            {
                await CreateContact(member, contactList);
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
                await DeleteContact(contact);
            }
        }

        public async Task UpdateEventEmail(Event @event, Email email, string emailId)
        {
            ContactList contactList = await GetEventContactList();

            EventCampaign campaign = new EventCampaign
            {
                ContactListId = contactList.Id,
                From = Settings.FromEmailAddress,
                FromName = Settings.FromName,
                HtmlContent = email.Body,
                Id = emailId,
                Name = $"Event: {@event.Name}",
                Subject = email.Subject
            };

            await UpdateCampaign(campaign);
        }

        public async Task UpdateMemberOptIn(Member member, bool optIn)
        {
            ChapterEmailSettings emailSettings = await _chapterRepository.GetChapterEmailSettings(member.ChapterId);

            await UpdateContactOptIn(member.EmailAddress, optIn);
        }

        protected abstract Task<string> CreateCampaign(EventCampaign campaign);

        protected abstract Task CreateContact(Member member, ContactList contactList);

        protected abstract Task DeleteContact(Contact contact);

        protected abstract Task<Contact> GetContact(string emailAddress);

        protected abstract Task<ContactList> GetContactList(string name);

        protected abstract Task<IReadOnlyCollection<Contact>> GetContacts(string contactListId);

        protected abstract Task<EventCampaignStats> GetEventStats(EventEmail eventEmail);

        protected abstract Task<IReadOnlyCollection<EventCampaignStats>> GetStats(IEnumerable<EventEmail> eventEmails);

        protected abstract Task SendCampaignEmail(string id);

        protected abstract Task SendTestCampaignEmail(string id, string to);

        protected abstract Task UpdateCampaign(EventCampaign campaign);

        protected abstract Task UpdateCampaignEmailContent(EventCampaign campaign);

        protected abstract Task UpdateContactOptIn(string emailAddress, bool optIn);

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

        private async Task<ContactList> GetEventContactList()
        {
            string listName = $"{Chapter.Name} Events";

            ContactList contactList = await GetContactList(listName);
            if (contactList == null)
            {
                throw new OdkServiceException($"Contact list not found: {listName}");
            }

            return contactList;
        }
    }
}
