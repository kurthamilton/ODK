using ODK.Core.Members;

namespace ODK.Data.Core.QueryBuilders;

public interface IMemberSubscriptionRecordQueryBuilder :
    IDatabaseEntityQueryBuilder<MemberSubscriptionRecord, IMemberSubscriptionRecordQueryBuilder>
{
    IMemberSubscriptionRecordQueryBuilder Current();

    IMemberSubscriptionRecordQueryBuilder ForChapter(Guid chapterId);

    IMemberSubscriptionRecordQueryBuilder ForExternalId(string externalId);

    IMemberSubscriptionRecordQueryBuilder ForInitiator(string initiatorId);

    IMemberSubscriptionRecordQueryBuilder ForMember(Guid memberId);

    IMemberSubscriptionRecordQueryBuilder InChapters(IEnumerable<Guid> chapterIds);

    /// <summary>
    /// Projects to the member's current subscription state (type, expiry, whether it's active recurring),
    /// yielding a query builder of that type. Apply filters (e.g. <see cref="Current"/>) before projecting.
    /// </summary>
    IQueryBuilder<MemberChapterSubscription> ToChapterSubscription();
}
