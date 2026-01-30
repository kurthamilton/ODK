using ODK.Core.Features;
using ODK.Data.Core;

namespace ODK.Services.Features;

public class FeatureService : OdkAdminServiceBase, IFeatureService
{
    private readonly IUnitOfWork _unitOfWork;

    public FeatureService(IUnitOfWork unitOfWork)
        : base(unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> AddFeature(MemberServiceRequest request, UpdateFeature model)
    {
        AssertMemberIsSiteAdmin(request.CurrentMember);

        _unitOfWork.FeatureRepository.Add(new Feature
        {
            CreatedUtc = DateTime.UtcNow,
            Description = model.Description,
            Id = Guid.NewGuid(),
            Name = model.Name
        });

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task DeleteFeature(MemberServiceRequest request, Guid featureId)
    {
        var feature = await GetSiteAdminRestrictedContent(request,
            x => x.FeatureRepository.GetById(featureId));

        _unitOfWork.FeatureRepository.Delete(feature);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Feature> GetFeature(MemberServiceRequest request, Guid featureId)
    {
        return await GetSiteAdminRestrictedContent(request,
            x => x.FeatureRepository.GetById(featureId));
    }

    public async Task<IReadOnlyCollection<Feature>> GetFeatures(MemberServiceRequest request)
    {
        return await GetSiteAdminRestrictedContent(request,
            x => x.FeatureRepository.GetAll());
    }

    public Task<Feature?> GetUnseeen(MemberServiceRequest request, string featureName) => _unitOfWork.FeatureRepository
        .GetUnseen(request.CurrentMember.Id, featureName)
        .Run();

    public async Task MarkAsSeen(MemberServiceRequest request, string featureName)
    {
        var memberId = request.CurrentMember.Id;

        var feature = await _unitOfWork.FeatureRepository.GetUnseen(memberId, featureName).Run();
        if (feature == null)
        {
            return;
        }

        _unitOfWork.FeatureRepository.MarkAsSeen(memberId, feature);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateFeature(MemberServiceRequest request, Guid featureId, UpdateFeature model)
    {
        var feature = await GetSiteAdminRestrictedContent(request,
            x => x.FeatureRepository.GetById(featureId));

        feature.Description = model.Description;
        feature.Name = model.Name;

        _unitOfWork.FeatureRepository.Update(feature);
        await _unitOfWork.SaveChangesAsync();
    }
}
