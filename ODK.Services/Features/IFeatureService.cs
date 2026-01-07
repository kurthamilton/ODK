using ODK.Core.Features;

namespace ODK.Services.Features;

public interface IFeatureService
{
    Task<ServiceResult> AddFeature(Guid currentMemberId, UpdateFeature model);

    Task DeleteFeature(Guid currentMemberId, Guid featureId);

    Task<Feature> GetFeature(Guid currentMemberId, Guid featureId);

    Task<IReadOnlyCollection<Feature>> GetFeatures(Guid currentMemberId);

    Task<Feature?> GetUnseeen(Guid memberId, string featureName);

    Task MarkAsSeen(Guid memberId, string featureName);

    Task UpdateFeature(Guid memberId, Guid featureId, UpdateFeature model);
}
