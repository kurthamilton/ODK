using ODK.Core.Referrals;

namespace ODK.Data.Core.Referrals;

/// <summary>
/// A campaign with its referral counts, projected in the same query so the index page doesn't run two
/// more per row.
/// </summary>
public class ReferralCampaignSummaryDto
{
    public required ReferralCampaign Campaign { get; init; }

    public required int CompletedCount { get; init; }

    public required int SentCount { get; init; }
}
