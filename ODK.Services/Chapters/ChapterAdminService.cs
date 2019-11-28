using System;
using System.Collections.Generic;
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

        public ChapterAdminService(IChapterRepository chapterRepository, ICacheService cacheService)
            : base(chapterRepository)
        {
            _cacheService = cacheService;
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

        public async Task UpdateChapterLinks(Guid currentMemberId, Guid chapterId, UpdateChapterLinks links)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            ChapterLinks update = new ChapterLinks(chapterId, links.Facebook, links.Instagram, links.Twitter, 0);
            await _chapterRepository.UpdateChapterLinks(update);

            _cacheService.RemoveVersionedItem<ChapterLinks>(chapterId);
        }
    }
}
