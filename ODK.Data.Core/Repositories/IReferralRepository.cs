using ODK.Core.Referrals;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Referrals;

namespace ODK.Data.Core.Repositories;

public interface IReferralRepository : IReadWriteRepository<Referral>
{
    IDeferredQueryMultiple<ReferralWithMemberDto> GetByCampaignId(Guid referralCampaignId);
}
