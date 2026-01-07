using ODK.Core.Platforms;
using ODK.Core.Web;

namespace ODK.Services;

public class MemberChapterServiceRequest : MemberServiceRequest
{
    public MemberChapterServiceRequest(
        Guid chapterId,
        Guid currentMemberId,
        IHttpRequestContext httpRequestContext,
        PlatformType platform)
        : base(currentMemberId, httpRequestContext, platform)
    {
        ChapterId = chapterId;
    }

    public MemberChapterServiceRequest(
        Guid chapterId,
        MemberServiceRequest request)
        : this(chapterId, request.CurrentMemberId, request.HttpRequestContext, request.Platform)
    {
    }

    public Guid ChapterId { get; }
}
