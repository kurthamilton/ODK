using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberPaymentSettingsRepository : WriteRepositoryBase<MemberPaymentSettings>, IMemberPaymentSettingsRepository
{
    public MemberPaymentSettingsRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<MemberPaymentSettings> GetByMemberId(Guid memberId) => Set()
        .Where(x => x.MemberId == memberId)
        .DeferredSingleOrDefault();

    protected override IQueryable<MemberPaymentSettings> Set() => base.Set()
        .Include(x => x.Currency);
}
