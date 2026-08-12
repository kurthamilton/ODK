using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterEmailSettingsRepository : ReadWriteRepositoryBase<ChapterEmailSettings>, IChapterEmailSettingsRepository
{
    public ChapterEmailSettingsRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<ChapterEmailSettings> GetByChapterIdOrDefault(Guid chapterId)
        => Set()
            .Where(x => x.ChapterId == chapterId)
            .DeferredSingleOrDefault();
}
