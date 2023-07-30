using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Emails;
using ODK.Services.Exceptions;
using ODK.Services.Recaptcha;

namespace ODK.Services.Chapters
{
    public class ChapterService : IChapterService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICacheService _cacheService;
        private readonly IChapterRepository _chapterRepository;
        private readonly IEmailService _emailService;
        private readonly IRecaptchaService _recaptchaService;

        public ChapterService(IChapterRepository chapterRepository, ICacheService cacheService, IEmailService emailService,
            IAuthorizationService authorizationService, IRecaptchaService recaptchaService)
        {
            _authorizationService = authorizationService;
            _cacheService = cacheService;
            _chapterRepository = chapterRepository;
            _emailService = emailService;
            _recaptchaService = recaptchaService;
        }
        
        public async Task<Chapter?> GetChapter(string name)
        {
            return await _chapterRepository.GetChapter(name);
        }

        public async Task<ChapterLinks?> GetChapterLinks(Guid chapterId)
        {
            ChapterLinks? links = await _chapterRepository.GetChapterLinks(chapterId);
            return links;
        }

        public async Task<ChapterMembershipSettings?> GetChapterMembershipSettings(Guid chapterId)
        {
            return await _cacheService.GetOrSetItem(
                () => _chapterRepository.GetChapterMembershipSettings(chapterId),
                chapterId);
        }

        public async Task<ChapterPaymentSettings> GetChapterPaymentSettings(Guid currentMemberId, Guid chapterId)
        {
            await _authorizationService.AssertMemberIsChapterMember(currentMemberId, chapterId);
            return await _chapterRepository.GetChapterPaymentSettings(chapterId);
        }
        
        public async Task<IReadOnlyCollection<ChapterProperty>> GetChapterProperties(Guid chapterId)
        {
            return await _chapterRepository.GetChapterProperties(chapterId);
        }
        
        public async Task<IReadOnlyCollection<ChapterPropertyOption>> GetChapterPropertyOptions(Guid chapterId)
        {
            return await _chapterRepository.GetChapterPropertyOptions(chapterId);
        }
        
        public async Task<IReadOnlyCollection<ChapterQuestion>> GetChapterQuestions(Guid chapterId)
        {
            IReadOnlyCollection<ChapterQuestion> questions = await _chapterRepository.GetChapterQuestions(chapterId);
            return questions
                .OrderBy(x => x.DisplayOrder)
                .ToArray();
        }

        public async Task<IReadOnlyCollection<Chapter>> GetChapters()
        {
            return await _chapterRepository.GetChapters();
        }

        public async Task<IReadOnlyCollection<ChapterSubscription>> GetChapterSubscriptions(Guid chapterId)
        {
            return await _chapterRepository.GetChapterSubscriptions(chapterId);
        }
        
        public async Task<ChapterTexts> GetChapterTexts(Guid chapterId)
        {
            return await _chapterRepository.GetChapterTexts(chapterId);
        }

        public async Task SendContactMessage(Guid chapterId, string fromAddress, string message, string recaptchaToken)
        {
            if (string.IsNullOrWhiteSpace(fromAddress) || string.IsNullOrWhiteSpace(message))
            {
                throw new OdkServiceException("Email address and message must be provided");
            }

            if (!MailUtils.ValidEmailAddress(fromAddress))
            {
                throw new OdkServiceException("Invalid email address format");
            }

            Chapter? chapter = await _chapterRepository.GetChapter(chapterId);
            if (chapter == null)
            {
                return;
            }

            bool verified = await _recaptchaService.Verify(recaptchaToken);
            if (!verified)
            {
                message = $"[FLAGGED AS SPAM] {message}";
            }

            ContactRequest contactRequest = new ContactRequest(Guid.Empty, chapter.Id, DateTime.UtcNow, fromAddress, message, false);
            await _chapterRepository.AddContactRequest(contactRequest);

            IDictionary<string, string> parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"from", fromAddress},
                {"message", HttpUtility.HtmlEncode(message)}
            };

            await _emailService.SendContactEmail(chapter, fromAddress, message, parameters);
        }
    }
}
