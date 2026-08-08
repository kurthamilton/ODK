using ODK.Services.Referrals.Models;
using ODK.Services.Referrals.ViewModels;

namespace ODK.Services.Referrals;

public interface IReferralAdminService
{
    Task<ServiceResult<Guid>> CreateCampaign(IMemberServiceRequest request, ReferralCampaignUpdateModel model);

    Task<ReferralCampaignAdminPageViewModel> GetCampaignViewModel(IMemberServiceRequest request, Guid campaignId);

    Task<ReferralCampaignsAdminPageViewModel> GetCampaignsViewModel(IMemberServiceRequest request);

    Task<ServiceResult> UpdateCampaign(
        IMemberServiceRequest request, Guid campaignId, ReferralCampaignUpdateModel model);
}
