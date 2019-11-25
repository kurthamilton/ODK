using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Services.Exceptions;

namespace ODK.Services.Chapters
{
    public class ChapterAdminService : IChapterAdminService
    {
        private readonly IChapterRepository _chapterRepository;

        public ChapterAdminService(IChapterRepository chapterRepository)
        {
            _chapterRepository = chapterRepository;
        }

        public async Task<IReadOnlyCollection<Chapter>> GetChapters(Guid memberId)
        {
            IReadOnlyCollection<Chapter> chapters = await _chapterRepository.GetAdminChapters(memberId);
            if (chapters.Count == 0)
            {
                throw new OdkNotAuthorizedException();
            }
            return chapters;
        }
    }
}
