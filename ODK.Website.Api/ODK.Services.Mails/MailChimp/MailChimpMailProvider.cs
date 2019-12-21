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
using ODK.Services.Events;

namespace ODK.Services.Mails.MailChimp
{
    public class MailChimpMailProvider : MailProviderBase
    {
        private readonly IMemberRepository _memberRepository;

        public MailChimpMailProvider(IChapterRepository chapterRepository, IMemberRepository memberRepository)
            : base(chapterRepository)
        {
            _memberRepository = memberRepository;
        }

        protected override async Task<string> CreateCampaign(string apiKey, EventCampaign campaign)
        {
            IMailChimpManager manager = CreateManager(apiKey);

            Campaign create = new Campaign
            {
                Recipients = new Recipient
                {
                    ListId = campaign.SubscriptionMemberGroupId
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

        protected override async Task<EventInvites> GetEventInvites(string apiKey, EventEmail eventEmail)
        {
            IMailChimpManager manager = CreateManager(apiKey);

            try
            {
                Campaign campaign = await manager.Campaigns.GetAsync(eventEmail.EmailProviderEmailId);

                return new EventInvites
                {
                    EventId = eventEmail.EventId,
                    Sent = campaign.EmailsSent ?? 0
                };
            }
            catch
            {
                return new EventInvites
                {
                    EventId = eventEmail.EventId,
                    Sent = 0
                };
            }
        }

        protected override async Task<IReadOnlyCollection<EventInvites>> GetInvites(string apiKey, IEnumerable<EventEmail> eventEmails)
        {
            throw new NotImplementedException();
        }

        protected override async Task<IReadOnlyCollection<SubscriptionMemberGroup>> GetSubscriptionMemberGroups(string apiKey)
        {
            IMailChimpManager manager = CreateManager(apiKey);

            IEnumerable<List> lists = await manager.Lists.GetAllAsync();

            return lists.Select(x => new SubscriptionMemberGroup
            {
                Id = x.Id,
                Name = x.Name
            }).ToArray();
        }

        protected override async Task SendCampaignEmail(string apiKey, EventCampaign campaign)
        {
            IMailChimpManager manager = CreateManager(apiKey);

            await manager.Campaigns.SendAsync(campaign.Id);
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

        private IMailChimpManager CreateManager(string apiKey)
        {
            return new MailChimpManager(apiKey);
        }
    }
}
