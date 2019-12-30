using System;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Services.Exceptions;

namespace ODK.Services
{
    public abstract class OdkAdminServiceBase
    {
        private readonly IChapterRepository _chapterRepository;

        protected OdkAdminServiceBase(IChapterRepository chapterRepository)
        {
            _chapterRepository = chapterRepository;
        }

        protected async Task AssertMemberIsChapterAdmin(Guid memberId, Guid chapterId)
        {
            ChapterAdminMember chapterAdminMember = await _chapterRepository.GetChapterAdminMember(chapterId, memberId);
            if (chapterAdminMember == null)
            {
                throw new OdkNotAuthorizedException();
            }
        }

        protected async Task AssertMemberIsChapterSuperAdmin(Guid memberId, Guid chapterId)
        {
            ChapterAdminMember chapterAdminMember = await _chapterRepository.GetChapterAdminMember(chapterId, memberId);
            if (chapterAdminMember == null || !chapterAdminMember.SuperAdmin)
            {
                throw new OdkNotAuthorizedException();
            }
        }
    }
}