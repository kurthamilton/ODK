using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;
public interface IChapterContactMessageRepository : IReadWriteRepository<ChapterContactMessage>
{
    IDeferredQueryMultiple<ChapterContactMessage> GetByChapterId(Guid chapterId);
}
