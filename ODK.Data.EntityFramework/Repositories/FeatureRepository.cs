using Microsoft.EntityFrameworkCore;
using ODK.Core.Features;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class FeatureRepository : ReadWriteRepositoryBase<Feature>, IFeatureRepository
{
    public FeatureRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<Feature> GetAll() => Set().DeferredMultiple();

    public IDeferredQuerySingle<Feature> GetByName(string name) => Set()
        .Where(x => x.Name == name)
        .DeferredSingle();

    public IDeferredQuerySingleOrDefault<Feature> GetUnseen(Guid memberId, string name)
    {
        var query =
            from feature in Set()
            from featureSeenByMember in Set<FeatureSeenByMember>()
                .Where(x => x.FeatureId == feature.Id && x.MemberId == memberId)
                .DefaultIfEmpty()
            where feature.Name == name
                && featureSeenByMember == null
            select feature;

        return query.DeferredSingleOrDefault();
    }

    public void MarkAsSeen(Guid memberId, Feature feature)
    {
        AddSingle(new FeatureSeenByMember
        {
            FeatureId = feature.Id,
            MemberId = memberId
        });
    }
}