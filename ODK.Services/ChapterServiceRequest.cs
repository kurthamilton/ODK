using ODK.Core.Chapters;

namespace ODK.Services;

public class ChapterServiceRequest : ServiceRequest, IChapterServiceRequest
{
    public required Chapter Chapter { get; init; }

    public static ChapterServiceRequest Create(
        Chapter chapter,
        IServiceRequest request)
    {
        return new ChapterServiceRequest
        {
            Chapter = chapter,
            CurrentMemberOrDefault = request.CurrentMemberOrDefault,
            HttpRequestContext = request.HttpRequestContext,
            Platform = request.Platform
        };
    }
}