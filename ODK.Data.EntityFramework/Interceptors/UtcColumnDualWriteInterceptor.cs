using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ODK.Data.EntityFramework.Interceptors;

/// <summary>
/// Transitional dual-write for the UTC date column-name standardisation. During the staggered rollout
/// each renamed column exists twice: the CLR property maps to the new "...Utc" column, and a shadow
/// property named "{Property}Column" maps to the legacy column. This copies the property value into its
/// shadow on every insert/update so both columns stay in sync until the legacy columns are dropped.
/// Remove once the legacy columns and their shadow properties are gone.
/// </summary>
internal sealed class UtcColumnDualWriteInterceptor : SaveChangesInterceptor
{
    private const string ShadowSuffix = "Column";

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        SyncShadowColumns(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        SyncShadowColumns(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void SyncShadowColumns(DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Added && entry.State != EntityState.Modified)
            {
                continue;
            }

            foreach (var shadow in entry.Metadata.GetProperties())
            {
                if (!shadow.IsShadowProperty() ||
                    shadow.ClrType != typeof(DateTime?) ||
                    !shadow.Name.EndsWith(ShadowSuffix, StringComparison.Ordinal))
                {
                    continue;
                }

                var sourceName = shadow.Name[..^ShadowSuffix.Length];

                if (entry.Metadata.FindProperty(sourceName) == null)
                {
                    continue;
                }

                entry.Property(shadow.Name).CurrentValue = entry.Property(sourceName).CurrentValue;
            }
        }
    }
}
