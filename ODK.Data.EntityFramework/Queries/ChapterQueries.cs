using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Queries;

internal static class ChapterQueries
{
    internal static IQueryable<Chapter> ForPlatform(this IQueryable<Chapter> query, PlatformType platform)
        => query
            .ConditionalWhere(x => x.Platform == platform, platform != PlatformType.Default);
}