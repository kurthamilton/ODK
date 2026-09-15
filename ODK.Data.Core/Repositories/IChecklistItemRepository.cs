using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IChecklistItemRepository : IReadWriteRepository<ChecklistItem>
{
    IDeferredQueryMultiple<ChecklistItem> GetAll();

    IDeferredQuerySingle<ChecklistItem> GetByType(ChecklistItemType type);
}
