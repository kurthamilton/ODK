using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IChapterRepository : IReadWriteRepository<Chapter>
{    
    IDeferredQueryMultiple<Chapter> GetAll();
    IDeferredQuerySingleOrDefault<Chapter> GetByName(string name);
}
