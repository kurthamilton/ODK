using Microsoft.EntityFrameworkCore;
using ODK.Core.Messages;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class SiteContactMessageReplyRepository : ReadWriteRepositoryBase<SiteContactMessageReply>, ISiteContactMessageReplyRepository
{
    public SiteContactMessageReplyRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<SiteContactMessageReply> GetAll() => Set()
        .DeferredMultiple();

    public IDeferredQueryMultiple<SiteContactMessageReply> GetBySiteContactMessageId(Guid siteContactMessageId) => Set()
        .Where(x => x.SiteContactMessageId == siteContactMessageId)
        .DeferredMultiple();

    protected override IQueryable<SiteContactMessageReply> Set() => base.Set()
        .Include(x => x.Member);
}
