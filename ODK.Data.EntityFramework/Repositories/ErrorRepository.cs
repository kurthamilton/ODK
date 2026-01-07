using ODK.Core.Logging;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ErrorRepository : ReadWriteRepositoryBase<Error>, IErrorRepository
{
    public ErrorRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<Error> GetErrors(int page, int pageSize) => Set()
        .OrderByDescending(x => x.CreatedUtc)
        .Page(page, pageSize)
        .DeferredMultiple();

    public IDeferredQueryMultiple<Error> GetErrors(int page, int pageSize, string exceptionMessage) => Set()
        .Where(x => x.ExceptionMessage == exceptionMessage)
        .OrderByDescending(x => x.CreatedUtc)
        .Page(page, pageSize)
        .DeferredMultiple();
}
