using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterRepository : ReadWriteRepositoryBase<Chapter>, IChapterRepository
{
    public ChapterRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<Chapter> GetAll() => Set()
        .OrderBy(x => x.DisplayOrder)
        .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<Chapter> GetByName(string name) => Set()
        .Where(x => x.Name == name)
        .DeferredSingleOrDefault();
}
