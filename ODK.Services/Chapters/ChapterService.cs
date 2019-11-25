using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Services.Exceptions;

namespace ODK.Services.Chapters
{
    public class ChapterService : IChapterService
    {
        private readonly IChapterRepository _chapterRepository;

        public ChapterService(IChapterRepository chapterRepository)
        {
            _chapterRepository = chapterRepository;
        }

        public async Task<IReadOnlyCollection<Chapter>> GetAdminChapters(Guid memberId)
        {
            IReadOnlyCollection<Chapter> chapters = await _chapterRepository.GetAdminChapters(memberId);
            if (chapters.Count == 0)
            {
                throw new OdkNotAuthorizedException();
            }
            return chapters;
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

        public async Task<IReadOnlyCollection<Chapter>> GetChapters()
        {
            return await _chapterRepository.GetChapters();
        }
    }
}
