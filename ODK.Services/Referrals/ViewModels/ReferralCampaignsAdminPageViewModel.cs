using ODK.Data.Core.Referrals;

namespace ODK.Services.Referrals.ViewModels;

public class ReferralCampaignsAdminPageViewModel
{
    public required IReadOnlyCollection<ReferralCampaignSummaryDto> Campaigns { get; init; }
}
