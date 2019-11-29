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

        public async Task UpdateChapterLinks(Guid currentMemberId, Guid chapterId, UpdateChapterLinks links)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            ChapterLinks update = new ChapterLinks(chapterId, links.Facebook, links.Instagram, links.Twitter, 0);
            await _chapterRepository.UpdateChapterLinks(update);

            _cacheService.RemoveVersionedItem<ChapterLinks>(chapterId);
        }
    }
}
