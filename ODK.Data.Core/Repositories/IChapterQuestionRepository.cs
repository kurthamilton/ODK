using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;
public interface IChapterQuestionRepository : IReadWriteRepository<ChapterQuestion>
{
    IDeferredQueryMultiple<ChapterQuestion> GetByChapterId(Guid chapterId);
}
