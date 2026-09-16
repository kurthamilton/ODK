using Microsoft.EntityFrameworkCore;
using ODK.Core.Venues;
using ODK.Data.Core.Repositories;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterVenueRepository : WriteRepositoryBase<ChapterVenue>, IChapterVenueRepository
{
    public ChapterVenueRepository(DbContext context)
        : base(context)
    {
    }
}
