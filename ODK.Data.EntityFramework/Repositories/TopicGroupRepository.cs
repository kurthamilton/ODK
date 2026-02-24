using Microsoft.EntityFrameworkCore;
using ODK.Core.Topics;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class TopicGroupRepository : ReadWriteRepositoryBase<TopicGroup>, ITopicGroupRepository
{
    public TopicGroupRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<TopicGroup> GetAll()
        => Set()
            .DeferredMultiple();
}