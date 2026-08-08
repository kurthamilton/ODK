using ODK.Core.Referrals;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Referrals;

namespace ODK.Data.Core.Repositories;

public interface IReferralCampaignRepository : IReadWriteRepository<ReferralCampaign>
{
    IDeferredQueryMultiple<ReferralCampaignSummaryDto> GetAllSummaries();

    /// <summary>
    /// The most recently created campaign that has not expired, or null when none is running. Kept
    /// deliberately simple - "which campaign is active" is expected to grow more rules later.
    /// </summary>
    IDeferredQuerySingleOrDefault<ReferralCampaign> GetMostRecentActive(DateTime utcNow);
}
