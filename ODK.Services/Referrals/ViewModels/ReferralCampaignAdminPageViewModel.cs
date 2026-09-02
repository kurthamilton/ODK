using ODK.Core.Referrals;
using ODK.Data.Core.Referrals;

namespace ODK.Services.Referrals.ViewModels;

public class ReferralCampaignAdminPageViewModel
{
    public required ReferralCampaign Campaign { get; init; }

    public required IReadOnlyCollection<ReferralWithMemberDto> Referrals { get; init; }

    /// <summary>The viewing site admin's zone: a site-wide page has no chapter to fall back to.</summary>
    public required TimeZoneInfo TimeZone { get; init; }
}
