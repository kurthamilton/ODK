using ODK.Core.Messages;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface ISiteContactMessageRepository : IReadWriteRepository<SiteContactMessage>
{
    IDeferredQueryMultiple<SiteContactMessage> GetAll();
}
