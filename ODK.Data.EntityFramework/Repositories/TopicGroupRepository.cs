using ODK.Core.Topics;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Caching;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class TopicGroupRepository : CachingReadWriteRepositoryBase<TopicGroup>, ITopicGroupRepository
{
    private static readonly EntityCache<Guid, TopicGroup> _cache = new DatabaseEntityCache<TopicGroup>();

    public TopicGroupRepository(OdkContext context)
        : base(context, _cache)
    {
    }

    public IDeferredQueryMultiple<TopicGroup> GetAll() => Set()
        .DeferredMultiple(
            _cache.GetAll,
            _cache.SetAll);
}
