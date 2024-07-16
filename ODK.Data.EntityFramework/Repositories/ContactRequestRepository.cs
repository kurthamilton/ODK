using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;
public class ContactRequestRepository : ReadWriteRepositoryBase<ContactRequest>, IContactRequestRepository
{
    public ContactRequestRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQueryMultiple<ContactRequest> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredMultiple();
}
