using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;
public interface IContactRequestRepository : IReadWriteRepository<ContactRequest>
{
    IDeferredQueryMultiple<ContactRequest> GetByChapterId(Guid chapterId);
}
