using ODK.Core.Emails;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IQueuedEmailRecipientRepository : IReadWriteRepository<QueuedEmailRecipient>
{
    IDeferredQueryMultiple<QueuedEmailRecipient> GetByQueuedEmailId(Guid queuedEmailId);
}
