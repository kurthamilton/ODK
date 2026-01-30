using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Exceptions;
using ODK.Services.Security;

namespace ODK.Services;

public abstract class OdkAdminServiceBase
{
    protected static readonly HtmlSanitizerOptions DefaultHtmlSantizerOptions = new HtmlSanitizerOptions
    {
        AllowLinks = true
    };

    private readonly IUnitOfWork _unitOfWork;

    protected OdkAdminServiceBase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    protected async Task AssertMemberIsChapterAdmin(
        ChapterAdminSecurable securable,
        Member member,
        Guid chapterId)
    {
        var chapterAdminMember = await _unitOfWork.ChapterAdminMemberRepository.GetByMemberId(member.Id, chapterId).Run();
        AssertMemberIsChapterAdmin(securable, member, chapterId, chapterAdminMember);
    }

    protected async Task AssertMemberIsChapterAdmin(ChapterAdminSecurable securable, MemberChapterServiceRequest request)
    {
        await AssertMemberIsChapterAdmin(securable, request.CurrentMember, request.Chapter.Id);
    }

    protected void AssertMemberIsChapterAdmin(
        ChapterAdminSecurable securable,
        Member member,
        Guid chapterId,
        ChapterAdminMember? chapterAdminMember)
    {
        var isChapterAdmin = MemberIsChapterAdmin(securable, member, chapterId, chapterAdminMember);
        if (!isChapterAdmin)
        {
            throw new OdkNotAuthorizedException();
        }
    }

    protected void AssertMemberIsChapterAdmin(
        ChapterAdminSecurable securable,
        MemberChapterServiceRequest request,
        ChapterAdminMember? chapterAdminMember) 
        => AssertMemberIsChapterAdmin(securable, request.CurrentMember, request.Chapter.Id, chapterAdminMember);

    protected async Task<T1> GetChapterAdminRestrictedContent<T1>(
        ChapterAdminSecurable securable,
        MemberChapterServiceRequest request,
        Func<IUnitOfWork, IDeferredQuery<T1>> query)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        var (chapterAdminMember, result1) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMember.Id, chapter.Id),
            query);

        AssertMemberIsChapterAdmin(securable, currentMember, chapter.Id, chapterAdminMember);

        return result1;
    }

    protected async Task<(T1, T2)> GetChapterAdminRestrictedContent<T1, T2>(
        ChapterAdminSecurable securable,
        MemberChapterServiceRequest request,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        var (chapterAdminMember, result1, result2) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMember.Id, chapter.Id),
            query1,
            query2);

        AssertMemberIsChapterAdmin(securable, currentMember, chapter.Id, chapterAdminMember);

        return (result1, result2);
    }

    protected async Task<(T1, T2, T3)> GetChapterAdminRestrictedContent<T1, T2, T3>(
        ChapterAdminSecurable securable,
        MemberChapterServiceRequest request,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        var (chapterAdminMember, result1, result2, result3) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMember.Id, chapter.Id),
            query1,
            query2,
            query3);

        AssertMemberIsChapterAdmin(securable, currentMember, chapter.Id, chapterAdminMember);

        return (result1, result2, result3);
    }

    protected async Task<(T1, T2, T3, T4)> GetChapterAdminRestrictedContent<T1, T2, T3, T4>(
        ChapterAdminSecurable securable,
        MemberChapterServiceRequest request,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        var (chapterAdminMember, result1, result2, result3, result4) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMember.Id, chapter.Id),
            query1,
            query2,
            query3,
            query4);

        AssertMemberIsChapterAdmin(securable, currentMember, chapter.Id, chapterAdminMember);

        return (result1, result2, result3, result4);
    }

    protected async Task<(T1, T2, T3, T4, T5)> GetChapterAdminRestrictedContent<T1, T2, T3, T4, T5>(
        ChapterAdminSecurable securable,
        MemberChapterServiceRequest request,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        var (chapterAdminMember, result1, result2, result3, result4, result5) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMember.Id, chapter.Id),
            query1,
            query2,
            query3,
            query4,
            query5);

        AssertMemberIsChapterAdmin(securable, currentMember, chapter.Id, chapterAdminMember);

        return (result1, result2, result3, result4, result5);
    }

    protected async Task<(T1, T2, T3, T4, T5, T6)> GetChapterAdminRestrictedContent<T1, T2, T3, T4, T5, T6>(
        ChapterAdminSecurable securable,
        MemberChapterServiceRequest request,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        var (chapterAdminMember, result1, result2, result3, result4, result5, result6) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMember.Id, chapter.Id),
            query1,
            query2,
            query3,
            query4,
            query5,
            query6);

        AssertMemberIsChapterAdmin(securable, currentMember, chapter.Id, chapterAdminMember);

        return (result1, result2, result3, result4, result5, result6);
    }

    protected async Task<(T1, T2, T3, T4, T5, T6, T7)> GetChapterAdminRestrictedContent<T1, T2, T3, T4, T5, T6, T7>(
        ChapterAdminSecurable securable,
        MemberChapterServiceRequest request,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6,
        Func<IUnitOfWork, IDeferredQuery<T7>> query7)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        var (chapterAdminMember, result1, result2, result3, result4, result5, result6, result7) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMember.Id, chapter.Id),
            query1,
            query2,
            query3,
            query4,
            query5,
            query6,
            query7);

        AssertMemberIsChapterAdmin(securable, currentMember, chapter.Id, chapterAdminMember);

        return (result1, result2, result3, result4, result5, result6, result7);
    }

    protected async Task<(T1, T2, T3, T4, T5, T6, T7, T8)> GetChapterAdminRestrictedContent<T1, T2, T3, T4, T5, T6, T7, T8>(
        ChapterAdminSecurable securable,
        MemberChapterServiceRequest request,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6,
        Func<IUnitOfWork, IDeferredQuery<T7>> query7,
        Func<IUnitOfWork, IDeferredQuery<T8>> query8)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        var (chapterAdminMember, result1, result2, result3, result4, result5, result6, result7, result8) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMember.Id, chapter.Id),
            query1,
            query2,
            query3,
            query4,
            query5,
            query6,
            query7,
            query8);

        AssertMemberIsChapterAdmin(securable, currentMember, chapter.Id, chapterAdminMember);

        return (result1, result2, result3, result4, result5, result6, result7, result8);
    }

    protected async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> GetChapterAdminRestrictedContent<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        ChapterAdminSecurable securable,
        MemberChapterServiceRequest request,
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
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        var (chapterAdminMember, result1, result2, result3, result4, result5, result6, result7, result8, result9) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMember.Id, chapter.Id),
            query1,
            query2,
            query3,
            query4,
            query5,
            query6,
            query7,
            query8,
            query9);

        AssertMemberIsChapterAdmin(securable, currentMember, chapter.Id, chapterAdminMember);

        return (result1, result2, result3, result4, result5, result6, result7, result8, result9);
    }

    protected async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)> GetChapterAdminRestrictedContent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        ChapterAdminSecurable securable,
        MemberChapterServiceRequest request,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6,
        Func<IUnitOfWork, IDeferredQuery<T7>> query7,
        Func<IUnitOfWork, IDeferredQuery<T8>> query8,
        Func<IUnitOfWork, IDeferredQuery<T9>> query9,
        Func<IUnitOfWork, IDeferredQuery<T10>> query10)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        var (chapterAdminMember,
            result1,
            result2,
            result3,
            result4,
            result5,
            result6,
            result7,
            result8,
            result9,
            result10) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMember.Id, chapter.Id),
            query1,
            query2,
            query3,
            query4,
            query5,
            query6,
            query7,
            query8,
            query9,
            query10);

        AssertMemberIsChapterAdmin(securable, currentMember, chapter.Id, chapterAdminMember);

        return (result1, result2, result3, result4, result5, result6, result7, result8, result9, result10);
    }

    protected async Task<T1> GetSiteAdminRestrictedContent<T1>(
        MemberServiceRequest request,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1)
    {
        var result1 = await query1(_unitOfWork).Run();

        AssertMemberIsSiteAdmin(request.CurrentMember);

        return result1;
    }

    protected async Task<(T1, T2)> GetSiteAdminRestrictedContent<T1, T2>(
        MemberServiceRequest request,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2)
    {
        var (result1, result2) = await _unitOfWork.RunAsync(
            query1,
            query2);

        AssertMemberIsSiteAdmin(request.CurrentMember);

        return (result1, result2);
    }

    protected async Task<(T1, T2, T3)> GetSiteAdminRestrictedContent<T1, T2, T3>(
        MemberServiceRequest request,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3)
    {
        var (result1, result2, result3) = await _unitOfWork.RunAsync(
            query1,
            query2,
            query3);

        AssertMemberIsSiteAdmin(request.CurrentMember);

        return (result1, result2, result3);
    }

    protected async Task<(T1, T2, T3, T4)> GetSiteAdminRestrictedContent<T1, T2, T3, T4>(
        MemberServiceRequest request,
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4)
    {
        var (result1, result2, result3, result4) = await _unitOfWork.RunAsync(
            query1,
            query2,
            query3,
            query4);

        AssertMemberIsSiteAdmin(request.CurrentMember);

        return (result1, result2, result3, result4);
    }

    protected void AssertMemberIsSiteAdmin(Member member)
    {
        if (!member.SiteAdmin)
        {
            throw new OdkNotAuthorizedException();
        }
    }

    private bool MemberIsChapterAdmin(
        ChapterAdminSecurable securable,
        Member member,
        Guid chapterId,
        ChapterAdminMember? chapterAdminMember)
    {
        if (member.SiteAdmin)
        {
            return true;
        }

        if (chapterAdminMember == null)
        {
            return false;
        }

        var role = securable.Role();
        return 
            chapterAdminMember.ChapterId == chapterId && 
            chapterAdminMember.MemberId == member.Id && 
            chapterAdminMember.HasAccessTo(role);
    }
}