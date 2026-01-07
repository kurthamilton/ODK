using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IChapterMembershipSettingsRepository : IWriteRepository<ChapterMembershipSettings>
{
    IDeferredQuerySingleOrDefault<ChapterMembershipSettings> GetByChapterId(Guid chapterId);
}
