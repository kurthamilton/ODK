using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IChapterRepository : IReadWriteRepository<Chapter>
{    
    IDeferredQueryMultiple<Chapter> GetAll();    
    IDeferredQueryMultiple<Chapter> GetByMemberId(Guid memberId);
    IDeferredQuerySingleOrDefault<Chapter> GetByName(string name);
    IDeferredQueryMultiple<Chapter> GetByOwnerId(Guid ownerId);
    IDeferredQuerySingleOrDefault<Chapter> GetBySlug(string slug);
}
