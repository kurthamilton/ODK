using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IChapterChecklistItemRepository : IWriteRepository<ChapterChecklistItem>
{
    IDeferredQueryMultiple<ChapterChecklistItem> GetByChapterId(Guid chapterId);

    IDeferredQuerySingleOrDefault<ChapterChecklistItem> GetByChapterId(Guid chapterId, ChecklistItemType type);
}
