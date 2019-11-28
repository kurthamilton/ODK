using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Caching;
using ODK.Core.Chapters;
using ODK.Services.Exceptions;
using ODK.Services.Mails;

namespace ODK.Services.Chapters
{
    public class ChapterService : IChapterService
    {
        private readonly ICache _cache;
        private readonly IChapterRepository _chapterRepository;
        private readonly IMailService _mailService;

        public ChapterService(IChapterRepository chapterRepository, ICache cache, IMailService mailService)
        {
            _cache = cache;
            _chapterRepository = chapterRepository;
            _mailService = mailService;
        }

        public async Task<Chapter> GetChapter(Guid id)
        {
            Chapter chapter = await _chapterRepository.GetChapter(id);
            if (chapter == null)
            {
                throw new OdkNotFoundException();
            }
            return chapter;
        }

        public async Task<ChapterLinks> GetChapterLinks(Guid chapterId)
        {
            ChapterLinks links = await _chapterRepository.GetChapterLinks(chapterId);
            if (links == null)
            {
                throw new OdkNotFoundException();
            }
            return links;
        }

        public async Task<IReadOnlyCollection<ChapterProperty>> GetChapterProperties(Guid chapterId)
        {
            return await _chapterRepository.GetChapterProperties(chapterId);
        }

        public async Task<IReadOnlyCollection<ChapterPropertyOption>> GetChapterPropertyOptions(Guid chapterId)
        {
            return await _chapterRepository.GetChapterPropertyOptions(chapterId);
        }

        public async Task<VersionedServiceResult<IReadOnlyCollection<Chapter>>> GetChapters(long? currentVersion)
        {
            int version = await _cache.GetOrSetVersion<Chapter>(_chapterRepository.GetChaptersVersion);
            if (version == currentVersion)
            {
                return new VersionedServiceResult<IReadOnlyCollection<Chapter>>(version);
            }

            IReadOnlyCollection<Chapter> chapters = await _cache.GetOrSetCollection(_chapterRepository.GetChapters, version);
            return new VersionedServiceResult<IReadOnlyCollection<Chapter>>(version, chapters);
        }

        public async Task SendContactMessage(Guid chapterId, string fromAddress, string message)
        {
            if (string.IsNullOrWhiteSpace(fromAddress) || string.IsNullOrWhiteSpace(message))
            {
                throw new OdkServiceException("Email address and message must be provided");
            }

            Chapter chapter = await GetChapter(chapterId);

            ContactRequest contactRequest = new ContactRequest(Guid.Empty, chapter.Id,DateTime.UtcNow, fromAddress, message, false);
            Guid contactRequestId = await _chapterRepository.AddContactRequest(contactRequest);

            IDictionary<string, string> parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"from", fromAddress},
                {"message", message}
            };

            if (await _mailService.SendChapterContactMail(chapter, parameters))
            {
                await _chapterRepository.ConfirmContactRequestSent(contactRequestId);
            }
        }
    }
}
