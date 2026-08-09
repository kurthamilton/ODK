using Microsoft.EntityFrameworkCore;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class SiteQuestionRepository : ReadWriteRepositoryBase<SiteQuestion>, ISiteQuestionRepository
{
    public SiteQuestionRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<SiteQuestion> GetByPlatform(PlatformType platform) => Set()
        .Where(x => x.Platform == platform)
        .OrderBy(x => x.DisplayOrder)
        .DeferredMultiple();

    public IDeferredQuery<bool> HasQuestions(PlatformType platform) => Set()
        .Where(x => x.Platform == platform)
        .DeferredAny();
}
