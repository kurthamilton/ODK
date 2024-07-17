using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;
public interface IChapterTextsRepository : IWriteRepository<ChapterTexts>
{
    IDeferredQuerySingle<ChapterTexts> GetByChapterId(Guid chapterId);
}
