using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterChecklistItemRepository
    : WriteRepositoryBase<ChapterChecklistItem>, IChapterChecklistItemRepository
{
    public ChapterChecklistItemRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<ChapterChecklistItem> GetByChapterId(Guid chapterId)
        => Set()
            .Where(x => x.ChapterId == chapterId)
            .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<ChapterChecklistItem> GetByChapterId(Guid chapterId, ChecklistItemType type)
        => Set()
            .Where(x => x.ChapterId == chapterId && x.ChecklistItemType == type)
            .DeferredSingleOrDefault();
}
