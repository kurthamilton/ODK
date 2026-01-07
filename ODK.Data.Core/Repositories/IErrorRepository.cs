using ODK.Core.Logging;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IErrorRepository : IReadWriteRepository<Error>
{
    IDeferredQueryMultiple<Error> GetErrors(int page, int pageSize);

    IDeferredQueryMultiple<Error> GetErrors(int page, int pageSize, string exceptionMessage);
}
