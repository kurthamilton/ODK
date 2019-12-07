using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Exceptions;
using ODK.Services.Mails;

namespace ODK.Services.Chapters
{
    public class ChapterService : IChapterService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICacheService _cacheService;
        private readonly IChapterRepository _chapterRepository;
        private readonly IMailService _mailService;

        public ChapterService(IChapterRepository chapterRepository, ICacheService cacheService, IMailService mailService,
            IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
            _cacheService = cacheService;
            _chapterRepository = chapterRepository;
            _mailService = mailService;
        }

        public async Task<VersionedServiceResult<Chapter>> GetChapter(long? currentVersion, Guid id)
        {
            VersionedServiceResult<IReadOnlyCollection<Chapter>> chapters = await GetChapters(currentVersion);
            if (chapters.Version == currentVersion)
            {
                return new VersionedServiceResult<Chapter>(chapters.Version);
            }

            Chapter chapter = chapters.Value.SingleOrDefault(x => x.Id == id);
            if (chapter == null)
            {
                throw new OdkNotFoundException();
            }
            return new VersionedServiceResult<Chapter>(chapters.Version, chapter);
        }

        public async Task<VersionedServiceResult<ChapterLinks>> GetChapterLinks(long? currentVersion, Guid chapterId)
        {
            return await _cacheService.GetOrSetVersionedItem(() => GetChapterLinks(chapterId), chapterId, currentVersion);
        }

        public async Task<ChapterPaymentSettings> GetChapterPaymentSettings(Guid currentMemberId, Guid chapterId)
        {
            await _authorizationService.AssertMemberIsChapterMember(currentMemberId, chapterId);
            return await _chapterRepository.GetChapterPaymentSettings(chapterId);
        }

        public async Task<VersionedServiceResult<IReadOnlyCollection<ChapterProperty>>> GetChapterProperties(long? currentVersion, Guid chapterId)
        {
            return await _cacheService.GetOrSetVersionedCollection(
                () => _chapterRepository.GetChapterProperties(chapterId),
                () => _chapterRepository.GetChapterPropertiesVersion(chapterId),
                currentVersion);
        }

        public async Task<VersionedServiceResult<IReadOnlyCollection<ChapterPropertyOption>>> GetChapterPropertyOptions(long? currentVersion, Guid chapterId)
        {
            return await _cacheService.GetOrSetVersionedCollection(
                () => _chapterRepository.GetChapterPropertyOptions(chapterId),
                () => _chapterRepository.GetChapterPropertyOptionsVersion(chapterId),
                currentVersion);
        }

        public async Task<VersionedServiceResult<IReadOnlyCollection<ChapterQuestion>>> GetChapterQuestions(long? currentVersion, Guid chapterId)
        {
            return await _cacheService.GetOrSetVersionedCollection(
                () => _chapterRepository.GetChapterQuestions(chapterId),
                () => _chapterRepository.GetChapterQuestionsVersion(chapterId),
                currentVersion);
        }

        public async Task<VersionedServiceResult<IReadOnlyCollection<Chapter>>> GetChapters(long? currentVersion)
        {
            return await _cacheService.GetOrSetVersionedCollection(
                _chapterRepository.GetChapters,
                _chapterRepository.GetChaptersVersion,
                currentVersion);
        }

        public async Task<IReadOnlyCollection<ChapterSubscription>> GetChapterSubscriptions(Guid chapterId)
        {
            return await _chapterRepository.GetChapterSubscriptions(chapterId);
        }

        public async Task SendContactMessage(Guid chapterId, string fromAddress, string message)
        {
            if (string.IsNullOrWhiteSpace(fromAddress) || string.IsNullOrWhiteSpace(message))
            {
                throw new OdkServiceException("Email address and message must be provided");
            }

            VersionedServiceResult<Chapter> chapter = await GetChapter(null, chapterId);

            ContactRequest contactRequest = new ContactRequest(Guid.Empty, chapter.Value.Id,DateTime.UtcNow, fromAddress, message, false);
            Guid contactRequestId = await _chapterRepository.AddContactRequest(contactRequest);

            IDictionary<string, string> parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"from", fromAddress},
                {"message", message}
            };

            if (await _mailService.SendChapterContactMail(chapter.Value, parameters))
            {
                await _chapterRepository.ConfirmContactRequestSent(contactRequestId);
            }
        }

        private async Task<ChapterLinks> GetChapterLinks(Guid chapterId)
        {
            ChapterLinks links = await _chapterRepository.GetChapterLinks(chapterId);
            if (links == null)
            {
                throw new OdkNotFoundException();
            }

            return links;
        }
    }
}
