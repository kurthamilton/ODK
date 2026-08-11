using ODK.Core.Features;

namespace ODK.Data.EntityFramework.Migrations.Enums;

/// <summary>
/// The enum types that have a lookup table, and the table each one maps to.
/// </summary>
/// <remarks>
/// Registration is explicit and an unregistered type throws. Deriving a name from the type instead
/// (pluralise, drop a "Type" suffix) silently produces a plausible-looking name for a table that
/// doesn't exist, and the migration then creates a second, orphaned one alongside the real table.
/// </remarks>
public static class EnumTables
{
    private static readonly IReadOnlyDictionary<Type, EnumTable> Tables = new Dictionary<Type, EnumTable>
    {
        [typeof(SiteFeatureType)] = new EnumTable
        {
            Name = "SiteFeatures"
        }
    };

    public static EnumTable Get<T>()
        where T : struct, Enum
        => Get(typeof(T));

    public static EnumTable Get(Type enumType)
    {
        if (!Tables.TryGetValue(enumType, out var table))
        {
            throw new ArgumentException(
                $"No enum table is registered for {enumType.Name}. Add one to {nameof(EnumTables)}.",
                nameof(enumType));
        }

        return table;
    }
}
