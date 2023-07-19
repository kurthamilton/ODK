using System;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Services.Exceptions;

namespace ODK.Services
{
    public abstract class OdkAdminServiceBase
    {
        protected OdkAdminServiceBase(IChapterRepository chapterRepository)
        {
            ChapterRepository = chapterRepository;
        }

        protected IChapterRepository ChapterRepository { get; }

        protected async Task AssertMemberIsChapterAdmin(Guid memberId, Guid chapterId)
        {
            bool isChapterAdmin = await MemberIsChapterAdmin(memberId, chapterId);
            if (!isChapterAdmin)
            {
                throw new OdkNotAuthorizedException();
            }
        }

        protected async Task<bool> MemberIsChapterAdmin(Guid memberId, Guid chapterId)
        {
            ChapterAdminMember chapterAdminMember = await ChapterRepository.GetChapterAdminMember(chapterId, memberId);
            return chapterAdminMember != null;
        }

        protected async Task AssertMemberIsChapterSuperAdmin(Guid memberId, Guid chapterId)
        {
            ChapterAdminMember chapterAdminMember = await ChapterRepository.GetChapterAdminMember(chapterId, memberId);
            if (chapterAdminMember == null || !chapterAdminMember.SuperAdmin)
            {
                throw new OdkNotAuthorizedException();
            }
        }
    }
}