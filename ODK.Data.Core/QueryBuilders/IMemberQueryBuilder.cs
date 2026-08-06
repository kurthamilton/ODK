using ODK.Core.Members;
using ODK.Data.Core.Members;
using ODK.Data.Core.QueryBuilders.QueryOptions;

namespace ODK.Data.Core.QueryBuilders;

public interface IMemberQueryBuilder : IDatabaseEntityQueryBuilder<Member, IMemberQueryBuilder>
{
    /// <summary>
    /// Members whose signup was flagged as likely automated. Uses the flag recorded at signup, so changing
    /// the score threshold doesn't change who this returns.
    /// </summary>
    IMemberQueryBuilder Flagged();

    IMemberQueryBuilder HasEmailAddress(string emailAddress);

    IMemberQueryBuilder HasEmailAddress(IEnumerable<string> emailAddresses);

    IMemberQueryBuilder InChapter(Guid chapterId);

    IMemberQueryBuilder InChapter(Guid chapterId, MemberChapterQueryOptions options);

    IMemberQueryBuilder InChapters(IEnumerable<Guid> chapterIds);

    IMemberQueryBuilder IsChapterOwner(Guid chapterId);

    IMemberQueryBuilder IsSiteAdmin();

    IMemberQueryBuilder Latest(int pageSize);

    IQueryBuilder<MemberWithAvatarDto> WithAvatar();
}