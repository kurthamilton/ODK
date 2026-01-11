using ODK.Core.Platforms;
using ODK.Core.Web;

namespace ODK.Services;

public class MemberChapterServiceRequest : MemberServiceRequest
{
    public MemberChapterServiceRequest()
    {
    }

    public required Guid ChapterId { get; init; }

    public static MemberChapterServiceRequest Create(Guid chapterId, MemberServiceRequest request)
        => Create(chapterId, request.CurrentMemberId, request.HttpRequestContext, request.Platform);

    public static MemberChapterServiceRequest Create(
        Guid chapterId,
        Guid currentMemberId,
        IHttpRequestContext httpRequestContext,
        PlatformType platform)
    {
        return new MemberChapterServiceRequest
        {
            ChapterId = chapterId,
            CurrentMemberId = currentMemberId,
            HttpRequestContext = httpRequestContext,
            Platform = platform
        };
    }
}