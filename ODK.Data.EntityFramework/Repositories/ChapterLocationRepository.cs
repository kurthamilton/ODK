using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.Repositories;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterLocationRepository : WriteRepositoryBase<ChapterLocation>, IChapterLocationRepository
{
    public ChapterLocationRepository(OdkContext context) 
        : base(context)
    {
    }

    public Task<ChapterLocation?> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .FirstOrDefaultAsync();
}
