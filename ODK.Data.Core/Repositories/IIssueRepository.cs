using ODK.Core.Issues;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IIssueRepository : IReadWriteRepository<Issue>
{
    IDeferredQueryMultiple<Issue> GetAll();

    IDeferredQueryMultiple<Issue> GetByMemberId(Guid memberId);
}
