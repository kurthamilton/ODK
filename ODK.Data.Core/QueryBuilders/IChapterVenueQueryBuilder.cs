using ODK.Core.Venues;

namespace ODK.Data.Core.QueryBuilders;

public interface IChapterVenueQueryBuilder : IQueryBuilder<ChapterVenue>
{
    IChapterVenueQueryBuilder Archived(bool value);

    IChapterVenueQueryBuilder ForChapter(Guid chapterId);

    IChapterVenueQueryBuilder ForVenue(Guid venueId);

    IVenueQueryBuilder ToVenue();
}