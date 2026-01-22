using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
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

    public IDeferredQuerySingleOrDefault<MemberPaymentSettings> GetByChapterId(Guid chapterId)
    {
        var query =
            from memberPaymentSettings in Set()
            from chapter in Set<Chapter>()
                .Where(x => x.OwnerId == memberPaymentSettings.MemberId)
            select memberPaymentSettings;

        return query.DeferredSingleOrDefault();
    }

    public IDeferredQuerySingleOrDefault<MemberPaymentSettings> GetByMemberId(Guid memberId)
        => Set()
            .Where(x => x.MemberId == memberId)
            .DeferredSingleOrDefault();

    protected override IQueryable<MemberPaymentSettings> Set() => base.Set()
        .Include(x => x.Currency);
}