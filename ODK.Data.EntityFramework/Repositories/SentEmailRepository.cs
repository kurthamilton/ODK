using ODK.Core.Emails;
using ODK.Data.Core.Repositories;

namespace ODK.Data.EntityFramework.Repositories;

public class SentEmailRepository : ReadWriteRepositoryBase<SentEmail>, ISentEmailRepository
{
    public SentEmailRepository(OdkContext context) 
        : base(context)
    {
    }
}
