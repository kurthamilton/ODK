using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Data.EntityFramework.Queries;

internal static class ChapterQueries
{
    internal static IQueryable<Chapter> ForPlatform(
        this IQueryable<Chapter> query,
        PlatformType platform,
        bool includeUnpublished)
    {
        if (platform != PlatformType.Default)
        {
            query = query
                .Where(x => x.Platform == platform);
        }

        if (platform == PlatformType.Default && !includeUnpublished)
        {
            query = query
                .Where(x => x.PublishedUtc != null);
        }

        return query;
    }
}