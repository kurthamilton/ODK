using ODK.Core.Referrals;
using ODK.Data.Core.Referrals;

namespace ODK.Services.Referrals.ViewModels;

public class ReferralCampaignAdminPageViewModel
{
    public required ReferralCampaign Campaign { get; init; }

    public required IReadOnlyCollection<ReferralWithMemberDto> Referrals { get; init; }
}
