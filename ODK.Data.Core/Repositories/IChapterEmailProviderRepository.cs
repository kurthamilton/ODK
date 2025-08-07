using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IChapterEmailProviderRepository : IReadWriteRepository<ChapterEmailProvider>
{
    IDeferredQueryMultiple<ChapterEmailProvider> GetByChapterId(Guid chapterId);

    IDeferredQueryMultiple<EmailProviderSummaryDto> GetEmailsSentToday(Guid chapterId);
}
