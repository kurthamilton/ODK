using ODK.Core.Topics;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface ITopicRepository : IReadWriteRepository<Topic>
{
    IDeferredQueryMultiple<Topic> GetAll();

    IDeferredQuerySingleOrDefault<Topic> GetByName(string name);
}
