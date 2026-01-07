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

    public IDeferredQuerySingle<InstagramImage> GetByPostId(Guid instagramPostId) => Set()
        .Where(x => x.InstagramPostId == instagramPostId)
        .DeferredSingle();
}
