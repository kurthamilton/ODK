using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class MemberSubscriptionRecordQueryBuilder :
    DatabaseEntityQueryBuilder<MemberSubscriptionRecord, IMemberSubscriptionRecordQueryBuilder>,
    IMemberSubscriptionRecordQueryBuilder
{
    internal MemberSubscriptionRecordQueryBuilder(DbContext context)
        : base(context)
    {
    }

    protected override IMemberSubscriptionRecordQueryBuilder Builder => this;

    public IMemberSubscriptionRecordQueryBuilder Current()
    {
        Query = Query.Where(x => x.IsCurrent);
        return this;
    }

    public IMemberSubscriptionRecordQueryBuilder ForChapter(Guid chapterId)
    {
        Query = Query.Where(x => x.ChapterId == chapterId);
        return this;
    }

    public IMemberSubscriptionRecordQueryBuilder ForExternalId(string externalId)
    {
        Query = Query.Where(x => x.ExternalId == externalId);
        return this;
    }

    public IMemberSubscriptionRecordQueryBuilder ForInitiator(string initiatorId)
    {
        Query = Query.Where(x => x.InitiatorId == initiatorId);
        return this;
    }

    public IMemberSubscriptionRecordQueryBuilder ForMember(Guid memberId)
    {
        Query = Query.Where(x => x.MemberId == memberId);
        return this;
    }

    public IMemberSubscriptionRecordQueryBuilder InChapters(IEnumerable<Guid> chapterIds)
    {
        Query = Query.Where(x => chapterIds.Contains(x.ChapterId));
        return this;
    }

    public IQueryBuilder<MemberChapterSubscription> ToChapterSubscription()
    {
        // Recurring is a property of the record's chapter subscription (a separate table), so it's resolved
        // with a correlated lookup here - this lets a caller read "active recurring" (Recurring + not
        // cancelled) straight off the projection rather than issuing a second query.
        var subscriptions = Set<ChapterSubscription>();

        var query = Query.Select(x => new MemberChapterSubscription
        {
            CancelledUtc = x.CancelledUtc,
            ChapterId = x.ChapterId,
            ExpiresUtc = x.ExpiresUtc,
            MemberId = x.MemberId,
            Recurring = subscriptions.Any(cs => cs.Id == x.ChapterSubscriptionId && cs.Recurring),
            Type = x.Type
        });

        return ProjectTo(query);
    }
}
