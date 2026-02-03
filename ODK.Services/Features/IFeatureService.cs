using ODK.Core.Features;
using ODK.Services.Features.Models;

namespace ODK.Services.Features;

public interface IFeatureService
{
    Task<ServiceResult> AddFeature(MemberServiceRequest request, FeatureUpdateModel model);

    Task DeleteFeature(MemberServiceRequest request, Guid featureId);

    Task<Feature> GetFeature(MemberServiceRequest request, Guid featureId);

    Task<IReadOnlyCollection<Feature>> GetFeatures(MemberServiceRequest request);

    Task<Feature?> GetUnseeen(MemberServiceRequest request, string featureName);

    Task MarkAsSeen(MemberServiceRequest request, string featureName);

    Task UpdateFeature(MemberServiceRequest request, Guid featureId, FeatureUpdateModel model);
}
