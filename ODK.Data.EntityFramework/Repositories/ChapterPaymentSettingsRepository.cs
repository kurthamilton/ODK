using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;
public class ChapterPaymentSettingsRepository : WriteRepositoryBase<ChapterPaymentSettings>, IChapterPaymentSettingsRepository
{
    public ChapterPaymentSettingsRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<ChapterPaymentSettings> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredSingleOrDefault();
}
