using ODK.Core.Issues;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IIssueMessageRepository : IReadWriteRepository<IssueMessage>
{
    IDeferredQueryMultiple<IssueMessage> GetByIssueId(Guid issueId);
}
