using Microsoft.EntityFrameworkCore;
using ODK.Core.Issues;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class IssueMessageRepository : ReadWriteRepositoryBase<IssueMessage>, IIssueMessageRepository
{
    public IssueMessageRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQueryMultiple<IssueMessage> GetByIssueId(Guid issueId) => Set()
        .Where(x => x.IssueId == issueId)
        .DeferredMultiple();

    protected override IQueryable<IssueMessage> Set() => base.Set()
        .Include(x => x.Member);
}
