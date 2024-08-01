using ODK.Core.Members;

namespace ODK.Data.EntityFramework.Queries;

internal static class MemberQueries
{
    private static readonly IReadOnlyCollection<SubscriptionType> ActiveSubscriptionTypes 
        = [SubscriptionType.Trial, SubscriptionType.Full, SubscriptionType.Partial];

    internal static IQueryable<Member> Current(this IQueryable<Member> query, 
        IQueryable<MemberSubscription> memberSubscriptionQuery, 
        Guid chapterId)
    {        
        query =
            from member in query.Where(x => x.Activated && !x.Disabled)
            where memberSubscriptionQuery.Any(x => 
                x.MemberId == member.Id && 
                x.ChapterId == chapterId &&
                ActiveSubscriptionTypes.Contains(x.Type))
            select member;

        return query;
    }

    internal static IQueryable<Member> InChapter(this IQueryable<Member> query, Guid chapterId)
        => query.Where(x => x.Chapters.Any(c => c.ChapterId == chapterId));
}
