using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberRepository : ReadWriteRepositoryBase<Member>, IMemberRepository
{
    public MemberRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<Member> GetAdminMembersByChapterId(Guid chapterId)
    {
        var query =
            from member in Set()
            from adminMember in Set<ChapterAdminMember>()
            where adminMember.MemberId == member.Id
                && adminMember.ChapterId == chapterId
            select member;
        return query.DeferredMultiple();
    }

    public IDeferredQuerySingleOrDefault<Member> GetByEmailAddress(string emailAddress) => Set()
        .Where(x => x.EmailAddress == emailAddress)
        .DeferredSingleOrDefault();

    public IDeferredQuerySingleOrDefault<Member> GetByIdOrDefault(Guid memberId, bool searchAll) => Query(searchAll)
        .Where(x => x.Id == memberId)
        .DeferredSingleOrDefault();

    public IDeferredQueryMultiple<Member> GetByChapterId(Guid chapterId, bool searchAll = false) => Query(searchAll)
        .Where(x => x.ChapterId == chapterId)
        .DeferredMultiple();

    public IDeferredQueryMultiple<Member> GetByChapterId(Guid chapterId, IEnumerable<Guid> memberIds) => Set()
        .Where(x => x.ChapterId == chapterId && memberIds.Contains(x.Id))
        .DeferredMultiple();

    public IDeferredQueryMultiple<Member> GetLatestByChapterId(Guid chapterId, int pageSize) => Set()
        .Where(x => x.ChapterId == chapterId)
        .OrderByDescending(x => x.CreatedUtc)
        .Take(pageSize)
        .DeferredMultiple();

    private IQueryable<Member> Query(bool searchAll)
    {
        if (searchAll)
        {
            return Set();
        }

        var subscriptionTypes = new[] { SubscriptionType.Trial, SubscriptionType.Full, SubscriptionType.Partial };
        var query = Set()
            .Where(x => x.Activated && !x.Disabled);

        return
            from member in query
            from memberSubscription in Set<MemberSubscription>()
            where memberSubscription.MemberId == member.Id
                && subscriptionTypes.Contains(memberSubscription.Type)
            select member;
    }
}
