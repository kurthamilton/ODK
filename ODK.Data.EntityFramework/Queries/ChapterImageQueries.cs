using ODK.Core.Chapters;
using ODK.Data.Core.Chapters;

namespace ODK.Data.EntityFramework.Queries;

internal static class ChapterImageQueries
{
    internal static IQueryable<ChapterImageMetadataDto> Metadata(this IQueryable<ChapterImage> query)
        => query
            .Select(x => new ChapterImageMetadataDto
            {
                ChapterId = x.ChapterId,
                MimeType = x.MimeType
            });
}