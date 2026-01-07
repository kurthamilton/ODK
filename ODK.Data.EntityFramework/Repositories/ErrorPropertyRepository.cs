using ODK.Core.Logging;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ErrorPropertyRepository : WriteRepositoryBase<ErrorProperty>, IErrorPropertyRepository
{
    public ErrorPropertyRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<ErrorProperty> GetByErrorId(Guid errorId) => Set()
        .Where(x => x.ErrorId == errorId)
        .OrderBy(x => x.Name)
        .DeferredMultiple();
}
