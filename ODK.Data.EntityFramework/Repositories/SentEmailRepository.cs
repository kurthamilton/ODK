using ODK.Core.Emails;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class SentEmailRepository : ReadWriteRepositoryBase<SentEmail>, ISentEmailRepository
{
    public SentEmailRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<SentEmail> GetByExternalId(string externalId)
        => Set()
            .Where(x => x.ExternalId == externalId)
            .DeferredSingleOrDefault();
}
