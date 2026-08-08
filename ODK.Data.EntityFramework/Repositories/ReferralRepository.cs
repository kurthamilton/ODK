using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Core.Referrals;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Referrals;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ReferralRepository : ReadWriteRepositoryBase<Referral>, IReferralRepository
{
    public ReferralRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<ReferralWithMemberDto> GetByCampaignId(Guid referralCampaignId)
    {
        var query =
            from referral in Set()
            from member in Set<Member>()
                .Where(x => x.Id == referral.MemberId)
            where referral.ReferralCampaignId == referralCampaignId
            orderby referral.CreatedUtc descending
            select new ReferralWithMemberDto
            {
                Member = member,
                Referral = referral
            };

        return query.DeferredMultiple();
    }
}
