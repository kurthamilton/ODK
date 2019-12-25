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
        public MailChimpMailProvider(IChapterRepository chapterRepository, IMemberRepository memberRepository)
            : base(chapterRepository, memberRepository)
        {
        }

        protected override async Task<string> CreateCampaign(string apiKey, EventCampaign campaign)
        {
            IMailChimpManager manager = CreateManager(apiKey);

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

        protected override Task CreateContact(string apiKey, OdkMember member, ContactList contactList)
        {
            throw new NotImplementedException();
        }

        protected override Task DeleteContact(string apiKey, Contact contact)
        {
            throw new NotImplementedException();
        }

        protected override Task<Contact> GetContact(string apiKey, string emailAddress)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ContactList> GetContactList(string apiKey, string name)
        {
            IMailChimpManager manager = CreateManager(apiKey);

            IEnumerable<List> lists = await manager.Lists.GetAllAsync();

            List list = lists
                .FirstOrDefault(x => string.Equals(name, x.Name, StringComparison.OrdinalIgnoreCase));

            return list != null ? new ContactList
            {
                Id = list.Id,
                Name = list.Name
            } : null;
        }

        protected override Task<IReadOnlyCollection<Contact>> GetContacts(string apiKey, string contactListId)
        {
            throw new NotImplementedException();
        }

        protected override async Task<EventCampaignStats> GetEventStats(string apiKey, EventEmail eventEmail)
        {
            IMailChimpManager manager = CreateManager(apiKey);

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

        protected override Task<IReadOnlyCollection<EventCampaignStats>> GetStats(string apiKey, IEnumerable<EventEmail> eventEmails)
        {
            throw new NotImplementedException();
        }

        protected override async Task SendCampaignEmail(string apiKey, string id)
        {
            IMailChimpManager manager = CreateManager(apiKey);

            await manager.Campaigns.SendAsync(id);
        }

        protected override Task SendTestCampaignEmail(string apiKey, string id, string to)
        {
            throw new NotImplementedException();
        }

        protected override Task UpdateCampaign(string apiKey, EventCampaign campaign)
        {
            throw new NotImplementedException();
        }

        protected override async Task UpdateCampaignEmailContent(string apiKey, EventCampaign campaign)
        {
            IMailChimpManager manager = CreateManager(apiKey);

            ContentRequest content = new ContentRequest
            {
                Html = campaign.HtmlContent
            };

            await manager.Content.AddOrUpdateAsync(campaign.Id, content);
        }

        protected override Task UpdateContactOptIn(string apiKey, string emailAddress, bool optIn)
        {
            throw new NotImplementedException();
        }

        private IMailChimpManager CreateManager(string apiKey)
        {
            return new MailChimpManager(apiKey);
        }
    }
}
