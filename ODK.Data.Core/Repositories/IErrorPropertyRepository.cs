using ODK.Core.Logging;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;
public interface IErrorPropertyRepository : IWriteRepository<ErrorProperty>
{
    IDeferredQueryMultiple<ErrorProperty> GetByErrorId(Guid errorId);
}
