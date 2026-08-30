using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ODK.Data.EntityFramework.Interceptors;

/// <summary>
/// Writes a value to the second column it is being copied into while a column rename is in flight.
/// <c>EntityTypeBuilderExtensions.DualWriteColumn</c> declares which columns are paired.
/// </summary>
/// <remarks>
/// Here rather than on the entities, so the domain says nothing about a rename it is not part of, and so
/// the whole mechanism comes out in one place once the old columns are gone.
/// <para>
/// It has to run for updates as well as inserts. An entity is attached and marked Modified whole rather
/// than by property (see <c>RepositoryBase.UpdateSingle</c>), so a mirror left alone is written as null
/// rather than skipped.
/// </para>
/// </remarks>
public class HtmlColumnMirrorInterceptor : SaveChangesInterceptor
{
    private const string MirrorSuffix = "Mirror";

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        FillMirrors(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        FillMirrors(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>The shadow property holding the copy of <paramref name="property"/>.</summary>
    internal static string MirrorName(string property) => property + MirrorSuffix;

    private static void FillMirrors(DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State is not EntityState.Added and not EntityState.Modified)
            {
                continue;
            }

            foreach (var property in entry.Metadata.GetProperties())
            {
                if (!property.IsShadowProperty()
                    || !property.Name.EndsWith(MirrorSuffix, StringComparison.Ordinal))
                {
                    continue;
                }

                var source = property.Name[..^MirrorSuffix.Length];
                entry.Property(property.Name).CurrentValue = entry.Property(source).CurrentValue;
            }
        }
    }
}
