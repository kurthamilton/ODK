using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Web;

namespace ODK.Services;

public class MemberChapterServiceRequest : MemberServiceRequest
{
    public MemberChapterServiceRequest()
    {
    }

    public required Chapter Chapter { get; init; }

    public static MemberChapterServiceRequest Create(Chapter chapter, MemberServiceRequest request)
        => Create(chapter, request.CurrentMember, request.HttpRequestContext, request.Platform);

    public static MemberChapterServiceRequest Create(Chapter chapter, Member member, ServiceRequest request)
        => Create(chapter, member, request.HttpRequestContext, request.Platform);

    public static MemberChapterServiceRequest Create(
        Chapter chapter,
        Member currentMember,
        IHttpRequestContext httpRequestContext,
        PlatformType platform)
    {
        return new MemberChapterServiceRequest
        {
            Chapter = chapter,
            CurrentMember = currentMember,
            CurrentMemberIdOrDefault = currentMember.Id,
            HttpRequestContext = httpRequestContext,
            Platform = platform
        };
    }
}