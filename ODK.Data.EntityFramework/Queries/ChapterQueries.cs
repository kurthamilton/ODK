using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Data.EntityFramework.Queries;

internal static class ChapterQueries
{
    internal static IQueryable<Chapter> ToPlatformChapters(this IQueryable<Chapter> query, 
        PlatformType platform)
    {
        if (platform != PlatformType.DrunkenKnitwits)
        {
            return query;
        }

        return query
            .Select(x => new Chapter
            {
                ApprovedUtc = x.ApprovedUtc,
                BannerImageUrl = x.BannerImageUrl,
                Id = x.Id,
                CountryId = x.CountryId,
                CreatedUtc = x.CreatedUtc,
                DisplayOrder = x.DisplayOrder,
                Name = x.Name.Replace(" Drunken Knitwits", ""),
                OwnerId = x.OwnerId,
                Platform = platform,
                PublishedUtc = x.PublishedUtc,
                RedirectUrl = x.RedirectUrl,
                Slug = x.Slug,
                TimeZone = x.TimeZone
            });
    }
}
