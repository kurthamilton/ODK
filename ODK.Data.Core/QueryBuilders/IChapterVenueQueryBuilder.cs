using ODK.Core.Venues;
using ODK.Data.Core.Venues;

namespace ODK.Data.Core.QueryBuilders;

public interface IChapterVenueQueryBuilder : IDatabaseEntityQueryBuilder<ChapterVenue, IChapterVenueQueryBuilder>
{
    IChapterVenueQueryBuilder Archived(bool value);

    IChapterVenueQueryBuilder ForChapter(Guid chapterId);

    IChapterVenueQueryBuilder ForVenue(Guid venueId);

    IVenueQueryBuilder ToVenue();

    IQueryBuilder<ChapterVenueWithEventSummaryDto> WithEventSummary();

    IQueryBuilder<ChapterVenueWithLocationDto> WithLocation();
}