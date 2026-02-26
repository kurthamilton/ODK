using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterMembershipSettingsRepository : WriteRepositoryBase<ChapterMembershipSettings>, IChapterMembershipSettingsRepository
{
    public ChapterMembershipSettingsRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<ChapterMembershipSettings> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredSingleOrDefault();
}