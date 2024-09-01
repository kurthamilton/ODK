using ODK.Core.Members;

namespace ODK.Data.EntityFramework.Queries;

internal static class MemberQueries
{
    private static readonly IReadOnlyCollection<SubscriptionType> ActiveSubscriptionTypes 
        = [SubscriptionType.Trial, SubscriptionType.Full, SubscriptionType.Partial];

    internal static IQueryable<Member> Current(this IQueryable<Member> query, 
        Guid chapterId)
    {        
        query =
            from member in query
            where member.Activated
                && member.Chapters.Any(x => x.ChapterId == chapterId)
            select member;

        return query;
    }

    internal static IQueryable<Member> InChapter(this IQueryable<Member> query, Guid chapterId)
        => query.Where(x => x.Chapters.Any(c => c.ChapterId == chapterId));

    internal static IQueryable<Member> Visible(this IQueryable<Member> query, Guid chapterId) => query
        .Where(member => !member.PrivacySettings.Any(x => x.ChapterId == chapterId && x.HideProfile));
}
