using ODK.Core.Venues;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.Core.Repositories;

public interface IChapterVenueRepository : IReadWriteRepository<ChapterVenue, IChapterVenueQueryBuilder>
{
}
