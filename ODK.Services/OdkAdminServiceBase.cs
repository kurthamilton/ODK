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

    protected async Task AssertMemberIsChapterAdmin(AdminServiceRequest request)
    {
        var (chapterId, memberId) = (request.ChapterId, request.CurrentMemberId);

        var (chapterAdminMembers, member) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByMemberId(memberId),
            x => x.MemberRepository.GetById(memberId));
        var chapterAdminMember = chapterAdminMembers.FirstOrDefault(x => x.ChapterId == chapterId);
        if (chapterAdminMember == null)
        {
            throw new OdkNotAuthorizedException();
        }

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

    protected async Task<T1> GetChapterAdminRestrictedContent<T1>(AdminServiceRequest request,
        Func<IUnitOfWork, IDeferredQuery<T1>> query)
    {
        var (chapterId, currentMemberId) = (request.ChapterId, request.CurrentMemberId);

        var (chapterAdminMembers, currentMember, result1) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            query);

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        return result1;
    }

    protected async Task<(T1, T2)> GetChapterAdminRestrictedContent<T1, T2>(AdminServiceRequest request,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2)
    {
        var (chapterId, currentMemberId) = (request.ChapterId, request.CurrentMemberId);

        var (chapterAdminMembers, currentMember, result1, result2) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            query1,
            query2);

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        return (result1, result2);
    }

    protected async Task<(T1, T2, T3)> GetChapterAdminRestrictedContent<T1, T2, T3>(
        AdminServiceRequest request,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3)
    {
        var (chapterId, currentMemberId) = (request.ChapterId, request.CurrentMemberId);

        var (chapterAdminMembers, currentMember, result1, result2, result3) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            query1,
            query2,
            query3);

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        return (result1, result2, result3);
    }

    protected async Task<(T1, T2, T3, T4)> GetChapterAdminRestrictedContent<T1, T2, T3, T4>(
        AdminServiceRequest request,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4)
    {
        var (chapterId, currentMemberId) = (request.ChapterId, request.CurrentMemberId);

        var (chapterAdminMembers, currentMember, result1, result2, result3, result4) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            query1,
            query2,
            query3,
            query4);

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        return (result1, result2, result3, result4);
    }

    protected async Task<(T1, T2, T3, T4, T5)> GetChapterAdminRestrictedContent<T1, T2, T3, T4, T5>(
        AdminServiceRequest request,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5)
    {
        var (chapterId, currentMemberId) = (request.ChapterId, request.CurrentMemberId);

        var (chapterAdminMembers, currentMember, result1, result2, result3, result4, result5) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            query1,
            query2,
            query3,
            query4,
            query5);

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        return (result1, result2, result3, result4, result5);
    }

    protected async Task<(T1, T2, T3, T4, T5, T6)> GetChapterAdminRestrictedContent<T1, T2, T3, T4, T5, T6>(
        AdminServiceRequest request,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6)
    {
        var (chapterId, currentMemberId) = (request.ChapterId, request.CurrentMemberId);

        var (chapterAdminMembers, currentMember, result1, result2, result3, result4, result5, result6) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            query1,
            query2,
            query3,
            query4,
            query5,
            query6);

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        return (result1, result2, result3, result4, result5, result6);
    }

    protected async Task<(T1, T2, T3, T4, T5, T6, T7)> GetChapterAdminRestrictedContent<T1, T2, T3, T4, T5, T6, T7>(
        AdminServiceRequest request,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6,
        Func<IUnitOfWork, IDeferredQuery<T7>> query7)
    {
        var (chapterId, currentMemberId) = (request.ChapterId, request.CurrentMemberId);

        var (chapterAdminMembers, currentMember, result1, result2, result3, result4, result5, result6, result7) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            query1,
            query2,
            query3,
            query4,
            query5,
            query6,
            query7);

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        return (result1, result2, result3, result4, result5, result6, result7);
    }

    protected async Task<(T1, T2, T3, T4, T5, T6, T7, T8)> GetChapterAdminRestrictedContent<T1, T2, T3, T4, T5, T6, T7, T8>(
        AdminServiceRequest request,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6,
        Func<IUnitOfWork, IDeferredQuery<T7>> query7,
        Func<IUnitOfWork, IDeferredQuery<T8>> query8)
    {
        var (chapterId, currentMemberId) = (request.ChapterId, request.CurrentMemberId);

        var (chapterAdminMembers, currentMember, result1, result2, result3, result4, result5, result6, result7, result8) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            query1,
            query2,
            query3,
            query4,
            query5,
            query6,
            query7,
            query8);

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        return (result1, result2, result3, result4, result5, result6, result7, result8);
    }

    protected async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> GetChapterAdminRestrictedContent<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        AdminServiceRequest request,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6,
        Func<IUnitOfWork, IDeferredQuery<T7>> query7,
        Func<IUnitOfWork, IDeferredQuery<T8>> query8,
        Func<IUnitOfWork, IDeferredQuery<T9>> query9)
    {
        var (chapterId, currentMemberId) = (request.ChapterId, request.CurrentMemberId);

        var (chapterAdminMembers, currentMember, result1, result2, result3, result4, result5, result6, result7, result8, result9) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            query1,
            query2,
            query3,
            query4,
            query5,
            query6,
            query7,
            query8,
            query9);

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        return (result1, result2, result3, result4, result5, result6, result7, result8, result9);
    }

    protected async Task<T1> GetSuperAdminRestrictedContent<T1>(Guid currentMemberId,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1)
    {
        var (currentMember, result1) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            query1);

        AssertMemberIsSuperAdmin(currentMember);

        return result1;
    }

    protected async Task<(T1, T2)> GetSuperAdminRestrictedContent<T1, T2>(Guid currentMemberId,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2)
    {
        var (currentMember, result1, result2) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            query1,
            query2);

        AssertMemberIsSuperAdmin(currentMember);

        return (result1, result2);
    }

    protected async Task<T1> GetSuperAdminRestrictedContent<T1>(AdminServiceRequest request,
        Func<IUnitOfWork, IDeferredQuery<T1>> query)
        => await GetSuperAdminRestrictedContent(request.CurrentMemberId, query);

    protected async Task<(T1, T2)> GetSuperAdminRestrictedContent<T1, T2>(AdminServiceRequest request,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2)
        => await GetSuperAdminRestrictedContent(request.CurrentMemberId, query1, query2);

    protected async Task<(T1, T2, T3)> GetSuperAdminRestrictedContent<T1, T2, T3>(Guid currentMemberId,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3)
    {
        var (currentMember, result1, result2, result3) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            query1,
            query2,
            query3);

        AssertMemberIsSuperAdmin(currentMember);

        return (result1, result2, result3);
    }

    protected async Task<(T1, T2, T3, T4)> GetSuperAdminRestrictedContent<T1, T2, T3, T4>(Guid currentMemberId,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4)
    {
        var (currentMember, result1, result2, result3, result4) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            query1,
            query2,
            query3,
            query4);

        AssertMemberIsSuperAdmin(currentMember);

        return (result1, result2, result3, result4);
    }

    protected bool MemberIsChapterAdmin(Member member, Guid chapterId, 
        IReadOnlyCollection<ChapterAdminMember> chapterAdminMembers)
    {
        return member.SuperAdmin || chapterAdminMembers
            .Any(x => x.ChapterId == chapterId && x.MemberId == member.Id);
    }

    protected void AssertMemberIsSuperAdmin(Member member)
    {
        if (!member.SuperAdmin)
        {
            throw new OdkNotAuthorizedException();
        }
    }

    protected async Task AssertMemberIsSuperAdmin(Guid memberId)
    {
        var member = await _unitOfWork.MemberRepository.GetById(memberId).RunAsync();
        AssertMemberIsSuperAdmin(member);
    }
}