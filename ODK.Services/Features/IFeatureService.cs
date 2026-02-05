using ODK.Core.Features;
using ODK.Services.Features.Models;

namespace ODK.Services.Features;

public interface IFeatureService
{
    Task<ServiceResult> AddFeature(IMemberServiceRequest request, FeatureUpdateModel model);

    Task DeleteFeature(IMemberServiceRequest request, Guid featureId);

    Task<Feature> GetFeature(IMemberServiceRequest request, Guid featureId);

    Task<IReadOnlyCollection<Feature>> GetFeatures(IMemberServiceRequest request);

    Task<Feature?> GetUnseeen(IMemberServiceRequest request, string featureName);

    Task MarkAsSeen(IMemberServiceRequest request, string featureName);

    Task UpdateFeature(IMemberServiceRequest request, Guid featureId, FeatureUpdateModel model);
}