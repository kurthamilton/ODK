using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChecklistItemRepository : ReadWriteRepositoryBase<ChecklistItem>, IChecklistItemRepository
{
    public ChecklistItemRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<ChecklistItem> GetAll()
        => Set()
            .OrderBy(x => x.DisplayOrder)
            .DeferredMultiple();

    public IDeferredQuerySingle<ChecklistItem> GetByType(ChecklistItemType type)
        => Set()
            .Where(x => x.Type == type)
            .DeferredSingle();
}
