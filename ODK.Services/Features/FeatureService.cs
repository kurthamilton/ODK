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

    public async Task<ServiceResult> AddFeature(Guid currentMemberId, UpdateFeature model)
    {
        await AssertMemberIsSuperAdmin(currentMemberId);

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

    public async Task DeleteFeature(Guid currentMemberId, Guid featureId)
    {
        var feature = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.FeatureRepository.GetById(featureId));

        _unitOfWork.FeatureRepository.Delete(feature);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Feature> GetFeature(Guid currentMemberId, Guid featureId)
    {
        var (currentMember, feature) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.FeatureRepository.GetById(featureId));

        AssertMemberIsSuperAdmin(currentMember);

        return feature;
    }

    public async Task<IReadOnlyCollection<Feature>> GetFeatures(Guid currentMemberId)
    {
        var (currentMember, features) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.FeatureRepository.GetAll());

        AssertMemberIsSuperAdmin(currentMember);

        return features;
    }

    public Task<Feature?> GetUnseeen(Guid memberId, string featureName) => _unitOfWork.FeatureRepository
        .GetUnseen(memberId, featureName)
        .Run();

    public async Task MarkAsSeen(Guid memberId, string featureName)
    {
        var feature = await _unitOfWork.FeatureRepository.GetUnseen(memberId, featureName).Run();
        if (feature == null)
        {
            return;
        }

        _unitOfWork.FeatureRepository.MarkAsSeen(memberId, feature);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateFeature(Guid memberId, Guid featureId, UpdateFeature model)
    {
        var (currentMember, feature) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(memberId),
            x => x.FeatureRepository.GetById(featureId));

        AssertMemberIsSuperAdmin(currentMember);

        feature.Description = model.Description;
        feature.Name = model.Name;

        _unitOfWork.FeatureRepository.Update(feature);
        await _unitOfWork.SaveChangesAsync();
    }
}
