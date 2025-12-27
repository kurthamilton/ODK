using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;
public interface IChapterPaymentSettingsRepository : IWriteRepository<ChapterPaymentSettings>
{
    IDeferredQuerySingle<ChapterPaymentSettings> GetByChapterId(Guid chapterId);
}
