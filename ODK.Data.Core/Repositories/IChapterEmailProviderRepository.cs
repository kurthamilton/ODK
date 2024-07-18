using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;
public interface IChapterEmailProviderRepository : IReadWriteRepository<ChapterEmailProvider>
{
    IDeferredQueryMultiple<ChapterEmailProvider> GetByChapterId(Guid chapterId);

    IDeferredQueryMultiple<ChapterEmailProviderSummaryDto> GetEmailsSentToday(Guid chapterId);
}
