using ODK.Core.Emails;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class QueuedEmailRecipientRepository : ReadWriteRepositoryBase<QueuedEmailRecipient>,
    IQueuedEmailRecipientRepository
{
    public QueuedEmailRecipientRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<QueuedEmailRecipient> GetByQueuedEmailId(Guid queuedEmailId) => Set()
        .Where(x => x.QueuedEmailId == queuedEmailId)
        .DeferredMultiple();
}
