using ODK.Core.Chapters;

namespace ODK.Data.Core.QueryBuilders;

public interface IChapterContactMessageQueryBuilder
    : IDatabaseEntityQueryBuilder<ChapterContactMessage, IChapterContactMessageQueryBuilder>
{
    IChapterContactMessageQueryBuilder ForChapter(Guid chapterId);

    IChapterContactMessageQueryBuilder ForSpamScore(bool isSpam, double threshold);
}