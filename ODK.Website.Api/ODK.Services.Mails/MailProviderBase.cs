using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Mail;
using ODK.Services.Events;

namespace ODK.Services.Mails
{
    public abstract class MailProviderBase : IMailProvider
    {
        private readonly IChapterRepository _chapterRepository;

        protected MailProviderBase(IChapterRepository chapterRepository)
        {
            _chapterRepository = chapterRepository;
        }

        public async Task<EventInvites> GetEventInvites(Event @event, EventEmail eventEmail)
        {
            ChapterEmailSettings emailSettings = await _chapterRepository.GetChapterEmailSettings(@event.ChapterId);
            EventInvites invites = await GetEventInvites(emailSettings.EmailApiKey, eventEmail);
            return invites ?? new EventInvites
            {
                EventId = @event.Id
            };
        }

        public async Task<IReadOnlyCollection<EventInvites>> GetInvites(Guid chapterId, IEnumerable<EventEmail> eventEmails)
        {
            ChapterEmailSettings emailSettings = await _chapterRepository.GetChapterEmailSettings(chapterId);
            return await GetInvites(emailSettings.EmailApiKey, eventEmails);
        }

        public async Task<string> SendEventEmail(Event @event, Email email)
        {
            ChapterEmailSettings emailSettings = await _chapterRepository.GetChapterEmailSettings(@event.ChapterId);

            IReadOnlyCollection<SubscriptionMemberGroup> subscriptionMemberGroups = await GetSubscriptionMemberGroups(emailSettings.EmailApiKey);

            EventCampaign campaign = new EventCampaign
            {
                From = emailSettings.FromEmailAddress,
                FromName = emailSettings.FromEmailName,
                HtmlContent = email.Body,
                Name = $"Event: {@event.Name}",
                Subject = email.Subject,
                SubscriptionMemberGroupId = subscriptionMemberGroups.FirstOrDefault()?.Id
            };

            campaign.Id = await CreateCampaign(emailSettings.EmailApiKey, campaign);

            await UpdateCampaignEmailContent(emailSettings.EmailApiKey, campaign);
            await SendCampaignEmail(emailSettings.EmailApiKey, campaign);

            return campaign.Id;
        }

        public Task SynchroniseMembers(Guid chapterId)
        {
            return Task.CompletedTask;
        }

        protected abstract Task<string> CreateCampaign(string apiKey, EventCampaign campaign);

        protected abstract Task<EventInvites> GetEventInvites(string apiKey, EventEmail eventEmail);

        protected abstract Task<IReadOnlyCollection<EventInvites>> GetInvites(string apiKey, IEnumerable<EventEmail> eventEmails);

        protected abstract Task<IReadOnlyCollection<SubscriptionMemberGroup>> GetSubscriptionMemberGroups(string apiKey);

        protected abstract Task SendCampaignEmail(string apiKey, EventCampaign campaign);

        protected abstract Task UpdateCampaignEmailContent(string apiKey, EventCampaign campaign);
    }
}
