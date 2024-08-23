using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IChapterPrivacySettingsRepository : IWriteRepository<ChapterPrivacySettings>
{
    IDeferredQuerySingleOrDefault<ChapterPrivacySettings> GetByChapterId(Guid chapterId);
}