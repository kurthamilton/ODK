using System;
using System.Collections.Generic;
using System.Linq;
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
            IReadOnlyCollection<Chapter> chapters = _cache.TryGetCollection<Chapter>();

            Chapter chapter = chapters != null ? chapters.SingleOrDefault(x => x.Id == id) : await _chapterRepository.GetChapter(id);
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

        public async Task<IReadOnlyCollection<Chapter>> GetChapters()
        {
            return await _cache.GetOrSetCollection(_chapterRepository.GetChapters);
        }
    }
}
