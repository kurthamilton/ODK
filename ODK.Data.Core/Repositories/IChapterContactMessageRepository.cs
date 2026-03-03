using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.Core.Repositories;

public interface IChapterContactMessageRepository : IReadWriteRepository<ChapterContactMessage, IChapterContactMessageQueryBuilder>
{
    IDeferredQueryMultiple<ChapterContactMessage> GetByChapterId(Guid chapterId);
}