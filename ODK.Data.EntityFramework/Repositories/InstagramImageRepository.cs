using Microsoft.EntityFrameworkCore;
using ODK.Core.SocialMedia;
using ODK.Data.Core.Repositories;

namespace ODK.Data.EntityFramework.Repositories;

public class InstagramImageRepository : ReadWriteRepositoryBase<InstagramImage>, IInstagramImageRepository
{
    public InstagramImageRepository(DbContext context)
        : base(context)
    {
    }
}