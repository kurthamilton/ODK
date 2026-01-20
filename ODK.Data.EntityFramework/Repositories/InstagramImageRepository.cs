using ODK.Core.SocialMedia;
using ODK.Data.Core.Repositories;

namespace ODK.Data.EntityFramework.Repositories;

public class InstagramImageRepository : ReadWriteRepositoryBase<InstagramImage>, IInstagramImageRepository
{
    public InstagramImageRepository(OdkContext context)
        : base(context)
    {
    }
}