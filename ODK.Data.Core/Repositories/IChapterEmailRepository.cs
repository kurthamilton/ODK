using ODK.Core.Emails;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;
public interface IChapterEmailRepository : IReadWriteRepository<ChapterEmail>
{
    IDeferredQueryMultiple<ChapterEmail> GetByChapterId(Guid chapterId);

    IDeferredQuerySingleOrDefault<ChapterEmail> GetByChapterId(Guid chapterId, EmailType type);
}
