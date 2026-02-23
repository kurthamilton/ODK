using ODK.Core.Chapters;
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

    IMemberQueryBuilder Owner();

    IQueryBuilder<ChapterSearchResultDto> Search(ChapterSearchCriteria criteria);

    IQueryBuilder<ChapterDto> ToChapterDto();
}
