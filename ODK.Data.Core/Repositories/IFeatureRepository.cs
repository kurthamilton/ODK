using ODK.Core.Features;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IFeatureRepository : IReadWriteRepository<Feature>
{
    IDeferredQueryMultiple<Feature> GetAll();

    IDeferredQuerySingle<Feature> GetByName(string name);

    IDeferredQuerySingleOrDefault<Feature> GetUnseen(Guid memberId, string name);

    void MarkAsSeen(Guid memberId, Feature feature);
}
