using ODK.Core.Chapters;
using ODK.Core.Messages;

namespace ODK.Data.Core.QueryBuilders;

public interface IChapterContactMessageQueryBuilder
    : IDatabaseEntityQueryBuilder<ChapterContactMessage, IChapterContactMessageQueryBuilder>
{
    IChapterContactMessageQueryBuilder ForChapter(Guid chapterId);

    IChapterContactMessageQueryBuilder ForStatus(MessageStatus status, double spamThreshold);
}