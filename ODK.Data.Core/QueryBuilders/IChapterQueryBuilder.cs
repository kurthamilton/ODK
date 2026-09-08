using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Data.Core.Chapters;

namespace ODK.Data.Core.QueryBuilders;

public interface IChapterQueryBuilder : IDatabaseEntityQueryBuilder<Chapter, IChapterQueryBuilder>
{
    IChapterQueryBuilder Approved();

    IChapterQueryBuilder ForAdminMember(Guid memberId);

    IChapterQueryBuilder ForEvent(Guid eventId);

    IChapterQueryBuilder ForMember(Guid memberId);

    IChapterQueryBuilder ForName(string name);

    IChapterQueryBuilder ForOwner(Guid ownerId);

    IChapterQueryBuilder ForSlug(string slug);

    /// <summary>
    /// Chapters this platform owns, and only those. Distinct from the platform a query is scoped to - what a
    /// platform *shows* takes in the other's groups on Group Squirrel - so this is what work that acts on a
    /// group rather than displaying it asks for.
    /// </summary>
    IChapterQueryBuilder OwnedBy(PlatformType platform);

    IMemberQueryBuilder Owner();

    /// <summary>
    /// Published chapters. Only needed over an unfiltered query: the platform-scoped ones already decide
    /// this for themselves, since what counts as visible differs between the two.
    /// </summary>
    IChapterQueryBuilder Published();

    IQueryBuilder<ChapterSearchResultDto> Search(ChapterSearchCriteria criteria);

    IQueryBuilder<ChapterDto> ToChapterDto();
}
