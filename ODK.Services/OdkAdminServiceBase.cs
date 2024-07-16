using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.Core;
using ODK.Services.Exceptions;

namespace ODK.Services;

public abstract class OdkAdminServiceBase
{
    private readonly IUnitOfWork _unitOfWork;
    protected OdkAdminServiceBase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    protected async Task AssertMemberIsChapterAdmin(Guid memberId, Guid chapterId)
    {
        bool isChapterAdmin = await MemberIsChapterAdmin(memberId, chapterId);
        if (!isChapterAdmin)
        {
            throw new OdkNotAuthorizedException();
        }
    }

    protected void AssertMemberIsChapterAdmin(Guid memberId, Guid chapterId, 
        IReadOnlyCollection<ChapterAdminMember> chapterAdminMembers)
    {
        bool isChapterAdmin = MemberIsChapterAdmin(memberId, chapterId, chapterAdminMembers);
        if (!isChapterAdmin)
        {
            throw new OdkNotAuthorizedException();
        }
    }

    protected async Task<bool> MemberIsChapterAdmin(Guid memberId, Guid chapterId)
    {
        var chapterAdminMembers = await _unitOfWork.ChapterAdminMemberRepository.GetByMemberId(memberId).RunAsync();
        return MemberIsChapterAdmin(memberId, chapterId, chapterAdminMembers);
    }

    protected bool MemberIsChapterAdmin(Guid memberId, Guid chapterId, IReadOnlyCollection<ChapterAdminMember> chapterAdminMembers)
    {
        return chapterAdminMembers.Any(x => x.ChapterId == chapterId && x.MemberId == memberId);
    }

    protected async Task AssertMemberIsChapterSuperAdmin(Guid memberId, Guid chapterId)
    {
        var (member, adminMembers) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(memberId),
            x => x.ChapterAdminMemberRepository.GetByMemberId(memberId));
        
        if (adminMembers.All(x => x.ChapterId != chapterId) || !member.SuperAdmin)
        {
            throw new OdkNotAuthorizedException();
        }
    }

    protected void AssertMemberIsChapterSuperAdmin(Member member, Guid chapterId, 
        IReadOnlyCollection<ChapterAdminMember> chapterAdminMembers)
    {
        AssertMemberIsChapterAdmin(member.Id, chapterId, chapterAdminMembers);

        if (!member.SuperAdmin)
        {
            throw new OdkNotAuthorizedException();
        }
    }
}