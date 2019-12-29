using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
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

        protected MailProviderBase(ChapterEmailProviderSettings settings, Chapter chapter,
            IChapterRepository chapterRepository, IMemberRepository memberRepository)
        {
            _chapterRepository = chapterRepository;
            _memberRepository = memberRepository;
            Chapter = chapter;
            Settings = settings;
        }

        public abstract string Name { get; }

        public ChapterEmailProviderSettings Settings { get; }

        protected Chapter Chapter { get; }

        public async Task<string> CreateEventEmail(Event @event, Email email)
        {
            EventCampaign campaign = await CreateCampaign(@event, email);

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
            Contact contact = await GetContact(member.EmailAddress);
            return contact?.OptIn ?? false;
        }

        public async Task SendEmail(ChapterAdminMember from, string to, Email email, IDictionary<string, string> parameters = null)
        {
            await SendEmail(from, new[] { to }, email, parameters);
        }

        public async Task SendEmail(ChapterAdminMember from, IEnumerable<string> to, Email email, IDictionary<string, string> parameters = null)
        {
            try
            {
                if (parameters != null)
                {
                    email = email.Interpolate(parameters);
                }

                MimeMessage message = await CreateMessage(from, to, email.Subject, email.Body);

                using SmtpClient client = new SmtpClient();
                await client.ConnectAsync(Settings.SmtpServer, Settings.SmtpPort, false);
                client.Authenticate(Settings.SmtpLogin, Settings.SmtpPassword);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch
            {
            }
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
            EventCampaign campaign = await CreateCampaign(@event, email);
            campaign.Id = emailId;
            await UpdateCampaign(campaign);
        }

        public async Task UpdateMemberEmailAddress(Member member, string newEmailAddress)
        {
            await UpdateContactEmailAddress(member.EmailAddress, newEmailAddress);
        }

        public async Task UpdateMemberOptIn(Member member, bool optIn)
        {
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

        protected abstract Task UpdateContactEmailAddress(string emailAddress, string newEmailAddress);

        protected abstract Task UpdateContactOptIn(string emailAddress, bool optIn);

        private async Task<EventCampaign> CreateCampaign(Event @event, Email email)
        {
            ContactList contactList = await GetEventContactList();

            IReadOnlyCollection<ChapterAdminMember> adminMembers = await _chapterRepository.GetChapterAdminMembers(Chapter.Id);

            return new EventCampaign
            {
                ContactListId = contactList.Id,
                From = Settings.FromEmailAddress,
                FromName = Settings.FromName,
                HtmlContent = email.Body,
                Name = $"Event: {@event.Name}",
                ReplyTo = adminMembers.FirstOrDefault(x => x.SendEventEmails)?.AdminEmailAddress,
                Subject = email.Subject
            };
        }

        private async Task<MimeMessage> CreateMessage(ChapterAdminMember from, IEnumerable<string> to, string subject, string body)
        {
            MimeMessage message = new MimeMessage
            {
                Body = new TextPart(TextFormat.Html)
                {
                    Text = body
                },
                Subject = subject
            };

            if (from != null)
            {
                Member member = await _memberRepository.GetMember(from.MemberId);
                message.From.Add(new MailboxAddress($"{member.FirstName} {member.LastName}", from.AdminEmailAddress));
            }
            else
            {
                message.From.Add(new MailboxAddress(Settings.FromName, Settings.FromEmailAddress));
            }

            foreach (string recipient in to)
            {
                message.To.Add(new MailboxAddress(recipient));
            }

            return message;
        }

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
