using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailChimp.Net;
using MailChimp.Net.Core;
using MailChimp.Net.Interfaces;
using MailChimp.Net.Models;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Members;
using OdkMember = ODK.Core.Members.Member;

namespace ODK.Services.Mails.MailChimp
{
    public class MailChimpMailProvider : MailProviderBase
    {
        public const string ProviderName = "MailChimp";

        public MailChimpMailProvider(ChapterEmailProviderSettings settings, Chapter chapter,
            IChapterRepository chapterRepository, IMemberRepository memberRepository)
            : base(settings, chapter, chapterRepository, memberRepository)
        {
        }

        public override string Name => ProviderName;

        protected override async Task<string> CreateCampaign(EventCampaign campaign)
        {
            IMailChimpManager manager = CreateManager();

            Campaign create = new Campaign
            {
                Recipients = new Recipient
                {
                    ListId = campaign.ContactListId
                },
                Settings = new Setting
                {
                    FromName = campaign.FromName,
                    ReplyTo = campaign.From,
                    SubjectLine = campaign.Subject,
                    Title = campaign.Subject,
                },
                Type = CampaignType.Regular
            };

            Campaign created = await manager.Campaigns.AddAsync(create);
            return created.Id;
        }

        protected override Task CreateContact(OdkMember member, ContactList contactList)
        {
            throw new NotImplementedException();
        }

        protected override Task DeleteContact(Contact contact)
        {
            throw new NotImplementedException();
        }

        protected override Task<Contact> GetContact(string emailAddress)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ContactList> GetContactList(string name)
        {
            IMailChimpManager manager = CreateManager();

            IEnumerable<List> lists = await manager.Lists.GetAllAsync();

            List list = lists
                .FirstOrDefault(x => string.Equals(name, x.Name, StringComparison.OrdinalIgnoreCase));

            return list != null ? new ContactList
            {
                Id = list.Id,
                Name = list.Name
            } : null;
        }

        protected override Task<IReadOnlyCollection<Contact>> GetContacts(string contactListId)
        {
            throw new NotImplementedException();
        }

        protected override async Task<EventCampaignStats> GetEventStats(EventEmail eventEmail)
        {
            IMailChimpManager manager = CreateManager();

            try
            {
                Campaign campaign = await manager.Campaigns.GetAsync(eventEmail.EmailProviderEmailId);

                return new EventCampaignStats
                {
                    EventId = eventEmail.EventId,
                    Sent = campaign.EmailsSent ?? 0
                };
            }
            catch
            {
                return new EventCampaignStats
                {
                    EventId = eventEmail.EventId
                };
            }
        }

        protected override Task<IReadOnlyCollection<EventCampaignStats>> GetStats(IEnumerable<EventEmail> eventEmails)
        {
            throw new NotImplementedException();
        }

        protected override async Task SendCampaignEmail(string id)
        {
            IMailChimpManager manager = CreateManager();

            await manager.Campaigns.SendAsync(id);
        }

        protected override Task SendTestCampaignEmail(string id, string to)
        {
            throw new NotImplementedException();
        }

        protected override Task UpdateCampaign(EventCampaign campaign)
        {
            throw new NotImplementedException();
        }

        protected override async Task UpdateCampaignEmailContent(EventCampaign campaign)
        {
            IMailChimpManager manager = CreateManager();

            ContentRequest content = new ContentRequest
            {
                Html = campaign.HtmlContent
            };

            await manager.Content.AddOrUpdateAsync(campaign.Id, content);
        }

        protected override Task UpdateContactOptIn(string emailAddress, bool optIn)
        {
            throw new NotImplementedException();
        }

        private IMailChimpManager CreateManager()
        {
            return new MailChimpManager(Settings.ApiKey);
        }
    }
}
