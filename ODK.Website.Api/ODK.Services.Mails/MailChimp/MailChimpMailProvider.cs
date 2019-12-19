using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailChimp.Net;
using MailChimp.Net.Core;
using MailChimp.Net.Interfaces;
using MailChimp.Net.Models;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Events;
using OdkEmail = ODK.Core.Mail.Email;
using OdkEvent = ODK.Core.Events.Event;
using OdkMember = ODK.Core.Members.Member;

namespace ODK.Services.Mails.MailChimp
{
    public class MailChimpMailProvider : IMailChimpMailProvider
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IMemberRepository _memberRepository;

        public MailChimpMailProvider(IChapterRepository chapterRepository, IMemberRepository memberRepository)
        {
            _chapterRepository = chapterRepository;
            _memberRepository = memberRepository;
        }

        public async Task<EventInvites> GetEventInvites(OdkEvent @event)
        {
            IMailChimpManager manager = await CreateManager(@event.ChapterId);

            try
            {
                Campaign campaign = await manager.Campaigns.GetAsync(@event.EmailProviderEmailId);

                return new EventInvites
                {
                    EventId = @event.Id,
                    Sent = campaign.EmailsSent ?? 0
                };
            }
            catch
            {
                return new EventInvites
                {
                    EventId = @event.Id,
                    Sent = 0
                };
            }
        }

        public async Task<string> SendEventEmail(OdkEvent @event, OdkEmail email)
        {
            Chapter chapter = await _chapterRepository.GetChapter(@event.ChapterId);
            ChapterEmailSettings emailSettings = await _chapterRepository.GetChapterEmailSettings(chapter.Id);

            IMailChimpManager manager = CreateManager(emailSettings);

            IEnumerable<List> lists = await manager.Lists.GetAllAsync();

            Campaign create = new Campaign
            {
                Recipients = new Recipient
                {
                    ListId = lists.First().Id
                },
                Settings = new Setting
                {
                    FromName = emailSettings.FromEmailName,
                    ReplyTo = emailSettings.FromEmailAddress,
                    SubjectLine = email.Subject,
                    Title = $"Event: {@event.Name}",
                },
                Type = CampaignType.Regular
            };

            Campaign campaign = await manager.Campaigns.AddAsync(create);

            ContentRequest content = new ContentRequest
            {
                Html = email.Body
            };

            await manager.Content.AddOrUpdateAsync(campaign.Id, content);

            await manager.Campaigns.SendAsync(campaign.Id);

            return campaign.Id;
        }

        public async Task SynchroniseMembers(Guid chapterId)
        {
            IReadOnlyCollection<OdkMember> members = await _memberRepository.GetMembers(chapterId);


        }

        private async Task<IMailChimpManager> CreateManager(Guid chapterId)
        {
            ChapterEmailSettings emailSettings = await _chapterRepository.GetChapterEmailSettings(chapterId);
            return CreateManager(emailSettings);
        }

        private IMailChimpManager CreateManager(ChapterEmailSettings emailSettings)
        {
            return new MailChimpManager(emailSettings.EmailApiKey);
        }
    }
}
