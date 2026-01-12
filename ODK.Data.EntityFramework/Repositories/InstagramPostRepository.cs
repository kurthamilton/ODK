using ODK.Core.SocialMedia;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class InstagramPostRepository : ReadWriteRepositoryBase<InstagramPost>, IInstagramPostRepository
{
    public InstagramPostRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<InstagramPost> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredMultiple();

    public IDeferredQueryMultiple<InstagramPost> GetByChapterId(Guid chapterId, int pageSize) => Set()
        .Where(x => x.ChapterId == chapterId)
        .OrderByDescending(x => x.Date)
        .Take(pageSize)
        .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<InstagramPost> GetLastPost(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .OrderByDescending(x => x.Date)
        .DeferredSingleOrDefault();
}
