using Microsoft.EntityFrameworkCore;
using ODK.Core.Topics;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Caching;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class TopicRepository : CachingReadWriteRepositoryBase<Topic>, ITopicRepository
{
    private static readonly EntityCache<Guid, Topic> _cache = new DatabaseEntityCache<Topic>();

    public TopicRepository(OdkContext context) 
        : base(context, _cache)
    {
    }

    public IDeferredQueryMultiple<Topic> GetAll() => Set()
        .DeferredMultiple(
            _cache.GetAll,
            _cache.SetAll);

    public IDeferredQuerySingleOrDefault<Topic> GetByName(string name) => Set()
        .Where(x => x.Name == name)
        .DeferredSingleOrDefault();

    protected override IQueryable<Topic> Set() => base.Set()
        .Include(x => x.TopicGroup);
}
