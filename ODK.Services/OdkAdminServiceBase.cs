using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
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
        var (chapterAdminMembers, member) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(memberId));
        bool isChapterAdmin = MemberIsChapterAdmin(member, chapterId, chapterAdminMembers);
        if (!isChapterAdmin)
        {
            throw new OdkNotAuthorizedException();
        }
    }

    protected void AssertMemberIsChapterAdmin(Member member, Guid chapterId, 
        IReadOnlyCollection<ChapterAdminMember> chapterAdminMembers)
    {
        bool isChapterAdmin = MemberIsChapterAdmin(member, chapterId, chapterAdminMembers);
        if (!isChapterAdmin)
        {
            throw new OdkNotAuthorizedException();
        }
    }

    protected async Task<T1> GetChapterAdminRestrictedContent<T1>(Guid currentMemberId, Guid chapterId,
        Func<IUnitOfWork, IDeferredQuery<T1>> query)
    {
        var (chapterAdminMembers, currentMember, result1) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            query);
        
        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        return result1;
    }

    protected async Task<(T1, T2)> GetChapterAdminRestrictedContent<T1, T2>(Guid currentMemberId, Guid chapterId,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2)
    {
        var (chapterAdminMembers, currentMember, result1, result2) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            query1,
            query2);

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        return (result1, result2);
    }

    protected async Task<(T1, T2, T3)> GetChapterAdminRestrictedContent<T1, T2, T3>(Guid currentMemberId, Guid chapterId,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3)
    {
        var (chapterAdminMembers, currentMember, result1, result2, result3) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            query1,
            query2,
            query3);

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        return (result1, result2, result3);
    }

    protected async Task<T1> GetSuperAdminRestrictedContent<T1>(Guid currentMemberId,
        Func<IUnitOfWork, IDeferredQuery<T1>> query)
    {
        var (currentMember, result1) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            query);

        AssertMemberIsSuperAdmin(currentMember);

        return result1;
    }

    protected bool MemberIsChapterAdmin(Member member, Guid chapterId, 
        IReadOnlyCollection<ChapterAdminMember> chapterAdminMembers)
    {
        return member.SuperAdmin || chapterAdminMembers
            .Any(x => x.ChapterId == chapterId && x.MemberId == member.Id);
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

    protected void AssertMemberIsSuperAdmin(Member member)
    {
        if (!member.SuperAdmin)
        {
            throw new OdkNotAuthorizedException();
        }
    }
}