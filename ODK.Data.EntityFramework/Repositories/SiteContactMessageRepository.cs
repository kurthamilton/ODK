using Microsoft.EntityFrameworkCore;
using ODK.Core.Messages;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class SiteContactMessageRepository
    : ReadWriteRepositoryBase<SiteContactMessage, ISiteContactMessageQueryBuilder>, ISiteContactMessageRepository
{
    public SiteContactMessageRepository(DbContext context)
        : base(context)
    {
    }

    public override ISiteContactMessageQueryBuilder Query()
        => CreateQueryBuilder(context => new SiteContactMessageQueryBuilder(context));
}