using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Web;

namespace ODK.Services;

public class ChapterServiceRequest : ServiceRequest
{
    public required Chapter Chapter { get; init; }

    public static ChapterServiceRequest Create(Chapter chapter, ServiceRequest request)
        => Create(
            chapter, 
            request.CurrentMemberIdOrDefault,
            request.HttpRequestContext, 
            request.Platform);

    public static ChapterServiceRequest Create(
        Chapter chapter,
        Guid? currentMemberIdOrDefault,
        IHttpRequestContext httpRequestContext,
        PlatformType platform)
    {
        return new ChapterServiceRequest
        {
            Chapter = chapter,
            CurrentMemberIdOrDefault = currentMemberIdOrDefault,
            HttpRequestContext = httpRequestContext,
            Platform = platform
        };
    }
}
