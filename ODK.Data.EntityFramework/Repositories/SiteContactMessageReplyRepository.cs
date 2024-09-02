using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ODK.Core.Messages;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class SiteContactMessageReplyRepository : ReadWriteRepositoryBase<SiteContactMessageReply>, ISiteContactMessageReplyRepository
{
    public SiteContactMessageReplyRepository(OdkContext context) 
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
