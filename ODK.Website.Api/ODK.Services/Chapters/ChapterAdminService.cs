using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Services.Caching;
using ODK.Services.Exceptions;
using ODK.Services.Mails;

namespace ODK.Services.Chapters
{
    public class ChapterAdminService : OdkAdminServiceBase, IChapterAdminService
    {
        private readonly ICacheService _cacheService;
        private readonly IChapterRepository _chapterRepository;
        private readonly IChapterService _chapterService;
        private readonly IMailProviderFactory _mailProviderFactory;

        public ChapterAdminService(IChapterRepository chapterRepository, ICacheService cacheService, IChapterService chapterService,
            IMailProviderFactory mailProviderFactory)
            : base(chapterRepository)
        {
            _cacheService = cacheService;
            _chapterRepository = chapterRepository;
            _chapterService = chapterService;
            _mailProviderFactory = mailProviderFactory;
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

        public async Task<ChapterEmailProviderSettings> GetChapterEmailProviderSettings(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterSuperAdmin(currentMemberId, chapterId);

            return await _chapterRepository.GetChapterEmailProviderSettings(chapterId);
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

        public Task<IReadOnlyCollection<string>> GetEmailProviders()
        {
            return _mailProviderFactory.GetProviders();
        }

        public async Task UpdateChapterEmailProviderSettings(Guid currentMemberId, Guid chapterId,
            UpdateChapterEmailProviderSettings emailProviderSettings)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            ChapterEmailProviderSettings current = await _chapterRepository.GetChapterEmailProviderSettings(chapterId);
            bool update = current != null;

            if (current == null)
            {
                current = new ChapterEmailProviderSettings(chapterId);
            }

            current.ApiKey = emailProviderSettings.ApiKey;
            current.EmailProvider = emailProviderSettings.EmailProvider;
            current.FromEmailAddress = emailProviderSettings.FromEmailAddress;
            current.FromName = emailProviderSettings.FromName;
            current.SmtpLogin = emailProviderSettings.SmtpLogin;
            current.SmtpPassword = emailProviderSettings.SmtpPassword;
            current.SmtpPort = emailProviderSettings.SmtpPort;
            current.SmtpServer = emailProviderSettings.SmtpServer;

            await ValidateChapterEmailProviderSettings(current);

            if (update)
            {
                await _chapterRepository.UpdateChapterEmailProviderSettings(current);
            }
            else
            {
                await _chapterRepository.AddChapterEmailProviderSettings(current);
            }
        }

        public async Task UpdateChapterEmailSettings(Guid currentMemberId, Guid chapterId, UpdateChapterEmailSettings emailSettings)
        {
            await AssertMemberIsChapterSuperAdmin(currentMemberId, chapterId);

            ChapterEmailSettings current = await _chapterRepository.GetChapterEmailSettings(chapterId);

            current.AdminEmailAddress = emailSettings.AdminEmailAddress;
            current.ContactEmailAddress = emailSettings.ContactEmailAddress;

            await ValidateChapterEmailSettings(current);

            await _chapterRepository.UpdateChapterEmailSettings(current);
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

        private async Task ValidateChapterEmailProviderSettings(ChapterEmailProviderSettings emailProviderSettings)
        {
            IReadOnlyCollection<string> emailProviders = await _mailProviderFactory.GetProviders();

            if (string.IsNullOrWhiteSpace(emailProviderSettings.ApiKey) ||
                string.IsNullOrWhiteSpace(emailProviderSettings.FromEmailAddress) ||
                string.IsNullOrWhiteSpace(emailProviderSettings.FromName) ||
                string.IsNullOrWhiteSpace(emailProviderSettings.SmtpLogin) ||
                string.IsNullOrWhiteSpace(emailProviderSettings.SmtpPassword) ||
                emailProviderSettings.SmtpPort == 0 ||
                !emailProviders.Contains(emailProviderSettings.EmailProvider))
            {
                throw new OdkServiceException("Some required fields are missing");
            }
        }

        private async Task ValidateChapterEmailSettings(ChapterEmailSettings emailSettings)
        {
            IReadOnlyCollection<string> emailProviders = await _mailProviderFactory.GetProviders();

            if (string.IsNullOrWhiteSpace(emailSettings.AdminEmailAddress) ||
                string.IsNullOrWhiteSpace(emailSettings.ContactEmailAddress))
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
