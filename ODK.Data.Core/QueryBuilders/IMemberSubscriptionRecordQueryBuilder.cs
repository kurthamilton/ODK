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

    /// <summary>
    /// Records naming a subscription at the payment provider. For a caller comparing what the provider holds
    /// against what we recorded - where a record naming a subscription the provider does not have is the
    /// finding, so the provider's ids cannot narrow the read.
    /// </summary>
    IMemberSubscriptionRecordQueryBuilder HasExternalId();

    IMemberSubscriptionRecordQueryBuilder InChapters(IEnumerable<Guid> chapterIds);

    /// <summary>
    /// Projects to the member's current subscription state (type, expiry, whether it's active recurring),
    /// yielding a query builder of that type. Apply filters (e.g. <see cref="Current"/>) before projecting.
    /// </summary>
    IQueryBuilder<MemberChapterSubscription> ToChapterSubscription();
}
