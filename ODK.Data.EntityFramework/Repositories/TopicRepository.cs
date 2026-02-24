using Microsoft.EntityFrameworkCore;
using ODK.Core.Topics;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class TopicRepository : ReadWriteRepositoryBase<Topic>, ITopicRepository
{
    public TopicRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<Topic> GetAll() 
        => Set()
            .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<Topic> GetByName(string name) => Set()
        .Where(x => x.Name == name)
        .DeferredSingleOrDefault();

    protected override IQueryable<Topic> Set() => base.Set()
        .Include(x => x.TopicGroup);
}
