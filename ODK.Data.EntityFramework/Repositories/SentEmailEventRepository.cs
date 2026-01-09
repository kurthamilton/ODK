using ODK.Core.Emails;
using ODK.Data.Core.Repositories;

namespace ODK.Data.EntityFramework.Repositories;

public class SentEmailEventRepository : ReadWriteRepositoryBase<SentEmailEvent>, ISentEmailEventRepository
{
    public SentEmailEventRepository(OdkContext context) 
        : base(context)
    {
    }
}
