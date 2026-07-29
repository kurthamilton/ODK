using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberSubscriptionRecordRepository : ReadWriteRepositoryBase<MemberSubscriptionRecord>, IMemberSubscriptionRecordRepository
{
    public MemberSubscriptionRecordRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<MemberSubscriptionRecord> GetByInitiatorIdOrDefault(string initiatorId)
        => Set()
            .Where(x => x.InitiatorId == initiatorId)
            .DeferredSingleOrDefault();

    public IDeferredQuerySingleOrDefault<MemberSubscriptionRecord> GetCurrentOrDefault(Guid memberId, Guid chapterId)
        => Set()
            .Where(x => x.MemberId == memberId && x.ChapterId == chapterId && x.IsCurrent)
            .DeferredSingleOrDefault();

    public IDeferredQuerySingleOrDefault<MemberSubscriptionRecord> GetLatest(Guid memberId, Guid chapterId)
    {
        var query =
            from memberSubscriptionRecord in Set()
            where memberSubscriptionRecord.MemberId == memberId && memberSubscriptionRecord.ChapterId == chapterId
            select memberSubscriptionRecord;

        return query
            .OrderByDescending(x => x.PurchasedUtc)
            .DeferredSingleOrDefault();
    }

    public IDeferredQuerySingleOrDefault<MemberSubscriptionRecord> GetLatestByExternalIdOrDefault(string externalId)
        => Set()
            .Where(x => x.ExternalId == externalId)
            .OrderByDescending(x => x.PurchasedUtc)
            .DeferredSingleOrDefault();

    public IDeferredQuery<bool> HasActiveRecurringSubscription(Guid memberId, Guid chapterId)
    {
        var query =
            from record in Set()
                .Where(x => x.MemberId == memberId && x.ChapterId == chapterId)
                .OrderByDescending(x => x.PurchasedUtc)
                .Take(1)
            from subscription in Set<ChapterSubscription>()
                .Where(x => x.Id == record.ChapterSubscriptionId && x.Recurring)
            where record.CancelledUtc == null
            select record;

        return query.DeferredAny();
    }
}