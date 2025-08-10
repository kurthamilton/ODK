using ODK.Core.Emails;
using ODK.Data.Core.Repositories;

namespace ODK.Data.EntityFramework.Repositories;

public class QueuedEmailRepository : ReadWriteRepositoryBase<QueuedEmail>, IQueuedEmailRepository
{
    public QueuedEmailRepository(OdkContext context) 
        : base(context)
    {
    }
}
