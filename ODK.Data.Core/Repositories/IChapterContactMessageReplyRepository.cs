using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IChapterContactMessageReplyRepository : IReadWriteRepository<ChapterContactMessageReply>
{
    IDeferredQueryMultiple<ChapterContactMessageReply> GetByChapterId(Guid chapterId);

    IDeferredQueryMultiple<ChapterContactMessageReply> GetByChapterContactMessageId(Guid chapterContactMessageId);
}
