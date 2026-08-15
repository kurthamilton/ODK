using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ODK.Core;
using ODK.Data.EntityFramework.Mapping;

namespace ODK.Data.EntityFramework.Interceptors;

/// <summary>
/// Copies an entity's key into the shadow property mapped to its new <c>Id</c> column, so a save writes the
/// key to both the old column and the new one while a rename is in flight - see <see cref="IdColumnRename"/>.
/// </summary>
/// <remarks>
/// Here rather than at the call sites: the entity's own property is the only key the code sets, and a second
/// value that has to be assigned everywhere an entity is constructed is one that will eventually be forgotten.
/// Modifying an existing row fills the new column too, so rows written before the column existed are corrected
/// as they are touched rather than only by the backfill.
/// </remarks>
public class IdColumnRenameInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        SetRenamedIdColumns(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        SetRenamedIdColumns(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void SetRenamedIdColumns(DbContext? context)
    {
        // Null where the save has no context to inspect; nothing to do either way.
        if (context == null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
            {
                continue;
            }

            // Only the entities whose map opted in - every other type has no such property.
            if (entry.Metadata.FindProperty(IdColumnRename.ShadowPropertyName) == null)
            {
                continue;
            }

            entry.Property(IdColumnRename.ShadowPropertyName).CurrentValue =
                entry.Property(nameof(IDatabaseEntity.Id)).CurrentValue;
        }
    }
}
