using Microsoft.EntityFrameworkCore;
using ODK.Core.Referrals;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Referrals;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ReferralCampaignRepository : ReadWriteRepositoryBase<ReferralCampaign>, IReferralCampaignRepository
{
    public ReferralCampaignRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<ReferralCampaign> GetMostRecentActive(DateTime utcNow) => Set()
        .Where(x => x.ExpiresUtc == null || x.ExpiresUtc > utcNow)
        .OrderByDescending(x => x.CreatedUtc)
        .Take(1)
        .DeferredSingleOrDefault();

    public IDeferredQueryMultiple<ReferralCampaignSummaryDto> GetAllSummaries()
    {
        var query =
            from campaign in Set()
            orderby campaign.CreatedUtc descending
            select new ReferralCampaignSummaryDto
            {
                Campaign = campaign,
                CompletedCount = Set<Referral>()
                    .Where(x => x.ReferralCampaignId == campaign.Id && x.CompletedUtc != null)
                    .Count(),
                SentCount = Set<Referral>()
                    .Where(x => x.ReferralCampaignId == campaign.Id)
                    .Count()
            };

        return query.DeferredMultiple();
    }
}
