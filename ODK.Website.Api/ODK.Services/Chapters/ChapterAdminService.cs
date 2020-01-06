using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Caching;
using ODK.Services.Exceptions;

namespace ODK.Services.Chapters
{
    public class ChapterAdminService : OdkAdminServiceBase, IChapterAdminService
    {
        private readonly ICacheService _cacheService;
        private readonly IChapterRepository _chapterRepository;
        private readonly IChapterService _chapterService;
        private readonly IMemberRepository _memberRepository;

        public ChapterAdminService(IChapterRepository chapterRepository, ICacheService cacheService,
            IChapterService chapterService, IMemberRepository memberRepository)
            : base(chapterRepository)
        {
            _cacheService = cacheService;
            _chapterRepository = chapterRepository;
            _chapterService = chapterService;
            _memberRepository = memberRepository;
        }

        public async Task AddChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            Member member = await _memberRepository.GetMember(memberId);
            if (member == null)
            {
                throw new OdkNotFoundException();
            }

            ChapterAdminMember existing = await _chapterRepository.GetChapterAdminMember(chapterId, memberId);
            if (existing != null)
            {
                return;
            }

            ChapterAdminMember adminMember = new ChapterAdminMember(chapterId, memberId);
            await _chapterRepository.AddChapterAdminMember(adminMember);
        }

        public async Task CreateChapterQuestion(Guid currentMemberId, Guid chapterId, CreateChapterQuestion question)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            int? displayOrder = question.DisplayOrder;
            if (displayOrder == null)
            {
                IReadOnlyCollection<ChapterQuestion> existing = await _chapterRepository.GetChapterQuestions(chapterId);
                displayOrder = existing.Count + 1;
            }

            ChapterQuestion create = new ChapterQuestion(Guid.Empty, chapterId, question.Name, question.Answer, displayOrder.Value);

            ValidateChapterQuestion(create);

            await _chapterRepository.CreateChapterQuestion(create);

            _cacheService.RemoveVersionedItem<ChapterQuestion>(chapterId);
        }

        public async Task CreateChapterSubscription(Guid currentMemberId, Guid chapterId, CreateChapterSubscription subscription)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            ChapterSubscription create = new ChapterSubscription(Guid.Empty, chapterId, subscription.Type, subscription.Name,
                subscription.Title, subscription.Description, subscription.Amount, subscription.Months);

            await ValidateChapterSubscription(create);

            await _chapterRepository.CreateChapterSubscription(create);
        }

        public async Task DeleteChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId)
        {
            ChapterAdminMember adminMember = await GetChapterAdminMember(currentMemberId, chapterId, memberId);
            if (adminMember.SuperAdmin)
            {
                return;
            }

            await _chapterRepository.DeleteChapterAdminMember(chapterId, memberId);
        }

        public async Task DeleteChapterSubscription(Guid currentMemberId, Guid id)
        {
            ChapterSubscription subscription = await GetChapterSubscription(currentMemberId, id);

            await _chapterRepository.DeleteChapterSubscription(subscription.Id);
        }

        public async Task<ChapterAdminMember> GetChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            ChapterAdminMember adminMember = await _chapterRepository.GetChapterAdminMember(chapterId, memberId);
            if (adminMember == null)
            {
                throw new OdkNotFoundException();
            }

            return adminMember;
        }

        public async Task<IReadOnlyCollection<ChapterAdminMember>> GetChapterAdminMembers(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _chapterRepository.GetChapterAdminMembers(chapterId);
        }

        public async Task<ChapterPaymentSettings> GetChapterPaymentSettings(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterSuperAdmin(currentMemberId, chapterId);

            return await _chapterRepository.GetChapterPaymentSettings(chapterId);
        }

        public async Task<IReadOnlyCollection<Chapter>> GetChapters(Guid currentMemberId)
        {
            IReadOnlyCollection<ChapterAdminMember> chapterAdminMembers = await _chapterRepository.GetChapterAdminMembersByMember(currentMemberId);
            if (chapterAdminMembers.Count == 0)
            {
                throw new OdkNotAuthorizedException();
            }
            VersionedServiceResult<IReadOnlyCollection<Chapter>> chapters = await _chapterService.GetChapters(null);
            return chapterAdminMembers
                .Select(x => chapters.Value.Single(chapter => chapter.Id == x.ChapterId))
                .ToArray();
        }

        public async Task<ChapterSubscription> GetChapterSubscription(Guid currentMemberId, Guid id)
        {
            ChapterSubscription subscription = await _chapterRepository.GetChapterSubscription(id);
            if (subscription == null)
            {
                throw new OdkNotFoundException();
            }

            await AssertMemberIsChapterAdmin(currentMemberId, subscription.ChapterId);

            return subscription;
        }

        public async Task<IReadOnlyCollection<ChapterSubscription>> GetChapterSubscriptions(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _chapterRepository.GetChapterSubscriptions(chapterId);
        }

        public async Task UpdateChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId,
            UpdateChapterAdminMember adminMember)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            ChapterAdminMember existing = await _chapterRepository.GetChapterAdminMember(chapterId, memberId);
            if (existing == null)
            {
                throw new OdkNotFoundException();
            }

            existing.AdminEmailAddress = adminMember.AdminEmailAddress;
            existing.ReceiveContactEmails = adminMember.ReceiveContactEmails;
            existing.ReceiveNewMemberEmails = adminMember.ReceiveNewMemberEmails;
            existing.SendNewMemberEmails = adminMember.SendNewMemberEmails;

            await _chapterRepository.UpdateChapterAdminMember(existing);
        }

        public async Task UpdateChapterLinks(Guid currentMemberId, Guid chapterId, UpdateChapterLinks links)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            ChapterLinks update = new ChapterLinks(chapterId, links.Facebook, links.Instagram, links.Twitter, 0);
            await _chapterRepository.UpdateChapterLinks(update);

            _cacheService.RemoveVersionedItem<ChapterLinks>(chapterId);
        }

        public async Task<ChapterPaymentSettings> UpdateChapterPaymentSettings(Guid currentMemberId, Guid chapterId,
            UpdateChapterPaymentSettings paymentSettings)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            ChapterPaymentSettings existing = await _chapterRepository.GetChapterPaymentSettings(chapterId);
            ChapterPaymentSettings update = new ChapterPaymentSettings(chapterId, paymentSettings.ApiPublicKey, paymentSettings.ApiSecretKey, existing.Provider);

            await _chapterRepository.UpdateChapterPaymentSettings(update);

            return update;
        }

        public async Task UpdateChapterSubscription(Guid currentMemberId, Guid id, CreateChapterSubscription subscription)
        {
            ChapterSubscription existing = await _chapterRepository.GetChapterSubscription(id);
            if (existing == null)
            {
                throw new OdkNotFoundException();
            }

            await AssertMemberIsChapterAdmin(currentMemberId, existing.ChapterId);

            existing.Amount = subscription.Amount;
            existing.Description = subscription.Description;
            existing.Months = subscription.Months;
            existing.Name = subscription.Name;
            existing.Title = subscription.Title;
            existing.Type = subscription.Type;

            await ValidateChapterSubscription(existing);

            await _chapterRepository.UpdateChapterSubscription(existing);
        }

        public async Task<ChapterTexts> UpdateChapterTexts(Guid currentMemberId, Guid chapterId, UpdateChapterTexts texts)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            if (string.IsNullOrWhiteSpace(texts.RegisterText) ||
                string.IsNullOrWhiteSpace(texts.WelcomeText))
            {
                throw new OdkServiceException("Some required fields are missing");
            }

            ChapterTexts update = new ChapterTexts(chapterId, texts.RegisterText, texts.WelcomeText);

            await _chapterRepository.UpdateChapterTexts(update);

            _cacheService.RemoveVersionedItem<ChapterTexts>(chapterId);

            return update;
        }

        private void ValidateChapterQuestion(ChapterQuestion question)
        {
            if (string.IsNullOrWhiteSpace(question.Name) ||
                string.IsNullOrWhiteSpace(question.Answer))
            {
                throw new OdkServiceException("Some required fields are missing");
            }
        }

        private async Task ValidateChapterSubscription(ChapterSubscription subscription)
        {
            if (!Enum.IsDefined(typeof(SubscriptionType), subscription.Type) || subscription.Type == SubscriptionType.None)
            {
                throw new OdkServiceException("Invalid type");
            }

            if (string.IsNullOrWhiteSpace(subscription.Description) ||
                string.IsNullOrWhiteSpace(subscription.Name) ||
                string.IsNullOrWhiteSpace(subscription.Title))
            {
                throw new OdkServiceException("Some required fields are missing");
            }

            if (subscription.Amount < 0)
            {
                throw new OdkServiceException("Amount cannot be less than 0");
            }

            if (subscription.Months < 1)
            {
                throw new OdkServiceException("Subscription must be for at least 1 month");
            }

            IReadOnlyCollection<ChapterSubscription> existing = await _chapterRepository.GetChapterSubscriptions(subscription.ChapterId);
            if (existing.Any(x => x.Id != subscription.Id && x.Name.Equals(subscription.Name)))
            {
                throw new OdkServiceException("A subscription with that name already exists");
            }
        }
    }
}
