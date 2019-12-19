using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Services.Caching;
using ODK.Services.Exceptions;

namespace ODK.Services.Chapters
{
    public class ChapterAdminService : OdkAdminServiceBase, IChapterAdminService
    {
        private readonly ICacheService _cacheService;
        private readonly IChapterRepository _chapterRepository;
        private readonly IChapterService _chapterService;

        public ChapterAdminService(IChapterRepository chapterRepository, ICacheService cacheService, IChapterService chapterService)
            : base(chapterRepository)
        {
            _cacheService = cacheService;
            _chapterRepository = chapterRepository;
            _chapterService = chapterService;
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

            _cacheService.RemoveVersionedCollection<ChapterQuestion>();
        }

        public async Task<ChapterEmailSettings> GetChapterEmailSettings(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _chapterRepository.GetChapterEmailSettings(chapterId);
        }

        public async Task<ChapterPaymentSettings> GetChapterPaymentSettings(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterSuperAdmin(currentMemberId, chapterId);

            return await _chapterRepository.GetChapterPaymentSettings(chapterId);
        }

        public async Task<IReadOnlyCollection<Chapter>> GetChapters(Guid memberId)
        {
            IReadOnlyCollection<ChapterAdminMember> chapterAdminMembers = await _chapterRepository.GetChapterAdminMembers(memberId);
            if (chapterAdminMembers.Count == 0)
            {
                throw new OdkNotAuthorizedException();
            }
            VersionedServiceResult<IReadOnlyCollection<Chapter>> chapters = await _chapterService.GetChapters(null);
            return chapterAdminMembers
                .Select(x => chapters.Value.Single(chapter => chapter.Id == x.ChapterId))
                .ToArray();
        }

        public async Task<Chapter> UpdateChapterDetails(Guid currentMemberId, Guid chapterId, UpdateChapterDetails details)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            if (string.IsNullOrWhiteSpace(details.WelcomeText))
            {
                throw new OdkServiceException("Welcome text is required");
            }

            await _chapterRepository.UpdateChapterWelcomeText(chapterId, details.WelcomeText);

            _cacheService.RemoveVersionedCollection<Chapter>();

            return await _chapterRepository.GetChapter(chapterId);
        }

        public async Task UpdateChapterEmailSettings(Guid currentMemberId, Guid chapterId, UpdateChapterEmailSettings emailSettings)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            ChapterEmailSettings current = await _chapterRepository.GetChapterEmailSettings(chapterId);

            ChapterEmailSettings update = new ChapterEmailSettings(chapterId, emailSettings.AdminEmailAddress, emailSettings.ContactEmailAddress,
                emailSettings.FromEmailAddress, emailSettings.FromEmailName, current.EmailProvider, emailSettings.EmailApiKey);

            ValidateChapterEmailSettings(update);

            await _chapterRepository.UpdateChapterEmailSettings(update);
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

        private void ValidateChapterEmailSettings(ChapterEmailSettings emailSettings)
        {
            if (string.IsNullOrWhiteSpace(emailSettings.AdminEmailAddress) ||
                string.IsNullOrWhiteSpace(emailSettings.ContactEmailAddress) ||
                string.IsNullOrWhiteSpace(emailSettings.FromEmailAddress) || 
                string.IsNullOrWhiteSpace(emailSettings.EmailApiKey))
            {
                throw new OdkServiceException("Some required fields are missing");
            }
        }

        private void ValidateChapterQuestion(ChapterQuestion question)
        {
            if (string.IsNullOrWhiteSpace(question.Name) ||
                string.IsNullOrWhiteSpace(question.Answer))
            {
                throw new OdkServiceException("Some required fields are missing");
            }
        }
    }
}
