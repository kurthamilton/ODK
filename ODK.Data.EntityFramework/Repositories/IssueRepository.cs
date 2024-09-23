using ODK.Core.Issues;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class IssueRepository : ReadWriteRepositoryBase<Issue>, IIssueRepository
{
    public IssueRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQueryMultiple<Issue> GetAll() => Set()
        .DeferredMultiple();

    public IDeferredQueryMultiple<Issue> GetByMemberId(Guid memberId) => Set()
        .Where(x => x.MemberId == memberId)
        .DeferredMultiple();
}
