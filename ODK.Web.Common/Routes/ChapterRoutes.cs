using System;
using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Web.Common.Routes;

public class ChapterRoutes
{
    public string Event(PlatformType platform, Chapter chapter, Guid eventId) => $"{Events(platform, chapter)}/{eventId}";

    public string Events(PlatformType platform, Chapter chapter) => ChapterPath(platform, chapter, "/events");

    private string ChapterPath(PlatformType platform, Chapter chapter, string path) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"/{chapter.Name}{path}".ToLowerInvariant(),
        _ => $"/groups/{chapter.Slug}{path}".ToLowerInvariant()
    };
}
