using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Caching;
using ODK.Core.Chapters;
using ODK.Services.Exceptions;

namespace ODK.Services.Chapters
{
    public class ChapterService : IChapterService
    {
        private readonly ICache _cache;
        private readonly IChapterRepository _chapterRepository;

        public ChapterService(IChapterRepository chapterRepository, ICache cache)
        {
            _cache = cache;
            _chapterRepository = chapterRepository;
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

        public async Task<VersionedServiceResult<IReadOnlyCollection<Chapter>>> GetChapters(int? currentVersion)
        {
            int version = await _cache.GetOrSetVersion<Chapter>(_chapterRepository.GetChaptersVersion);
            if (version == currentVersion)
            {
                return new VersionedServiceResult<IReadOnlyCollection<Chapter>>(version);
            }

            IReadOnlyCollection<Chapter> chapters = await _cache.GetOrSetCollection(_chapterRepository.GetChapters, version);
            return new VersionedServiceResult<IReadOnlyCollection<Chapter>>(chapters, version);
        }
    }
}
