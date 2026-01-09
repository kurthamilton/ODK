using ODK.Core.Emails;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface ISentEmailRepository : IReadWriteRepository<SentEmail>
{
    IDeferredQuerySingleOrDefault<SentEmail> GetByExternalId(string externalId);
}
