using ODK.Core.Members;
using ODK.Data.Core.QueryBuilders.Members;

namespace ODK.Data.EntityFramework.Queries;

internal static class MemberQueries
{
    internal static IQueryable<Member> InChapter(
        this IQueryable<Member> query, Guid chapterId, MemberChapterQueryOptions? options = null)
    {
        var includeHidden = options?.IncludeHidden == true;
        var includeInactive = options?.IncludeInactive == true;

        return query
            .Where(member =>
                (includeInactive || member.Activated) &&
                member.Chapters.Any(
                    memberChapter =>
                        memberChapter.ChapterId == chapterId &&
                        (includeHidden || !memberChapter.HideProfile)));
    }
}
