using ODK.Core.SocialMedia;
using ODK.Data.Core.Deferred;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;
public class InstagramImageRepository : WriteRepositoryBase<InstagramImage>, IInstagramImageRepository
{
    public InstagramImageRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<InstagramImage> GetByPostId(Guid instagramPostId) => Set()
        .Where(x => x.InstagramPostId == instagramPostId)
        .DeferredSingleOrDefault();
}
