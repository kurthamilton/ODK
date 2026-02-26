using Microsoft.EntityFrameworkCore;
using ODK.Core.Messages;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class SiteContactMessageRepository : ReadWriteRepositoryBase<SiteContactMessage>, ISiteContactMessageRepository
{
    public SiteContactMessageRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<SiteContactMessage> GetAll() => Set()
        .DeferredMultiple();
}