using ODK.Core.Utils;

namespace ODK.Data.EntityFramework.Migrations.Enums;

/// <summary>
/// Builds the raw SQL that keeps an enum lookup table in step with its enum.
/// </summary>
/// <remarks>
/// Every statement is guarded, because these tables are not part of the EF model: they exist in
/// databases that pre-date the migration baseline but not in one built from the migrations alone,
/// so the same migration has to be a no-op against the former and do real work against the latter.
/// </remarks>
public static class EnumTableSql
{
    private const int NameColumnLength = 100;

    /// <summary>
    /// Points an existing column at the enum table. The guard matches on the relationship rather
    /// than the constraint name, so a foreign key added by hand under a different name still
    /// counts - matching on the name would add a second one alongside it.
    /// </summary>
    public static string AddForeignKey<T>(string table, string column)
        where T : struct, Enum
    {
        var enumTable = EnumTables.Get<T>();
        var constraintName = $"FK_{table}_{enumTable.Name}_{column}";

        return Join(
            "IF NOT EXISTS (",
            "    SELECT 1",
            ForeignKeyMatch("    ", table, enumTable.Name, column) + ")",
            "BEGIN",
            $"    ALTER TABLE {Identifier(table)} ADD CONSTRAINT {Identifier(constraintName)}",
            $"        FOREIGN KEY ({Identifier(column)}) REFERENCES {Identifier(enumTable.Name)} ({Identifier(enumTable.IdColumnName)});",
            "END");
    }

    /// <summary>
    /// Creates the table if it is missing. The id column is not an identity - the values are the
    /// enum's own numbers, written explicitly, so that the number stays the contract.
    /// </summary>
    public static string CreateTable<T>()
        where T : struct, Enum
    {
        var table = EnumTables.Get<T>();

        return Join(
            $"IF OBJECT_ID({Literal(table.Name)}, N'U') IS NULL",
            "BEGIN",
            $"    CREATE TABLE {Identifier(table.Name)} (",
            $"        {Identifier(table.IdColumnName)} int NOT NULL,",
            $"        [Name] nvarchar({NameColumnLength}) NOT NULL,",
            $"        CONSTRAINT {Identifier($"PK_{table.Name}")} PRIMARY KEY ({Identifier(table.IdColumnName)}),",
            $"        CONSTRAINT {Identifier($"UQ_{table.Name}_Name")} UNIQUE ([Name])",
            "    );",
            "END");
    }

    public static string Delete<T>(params T[] values)
        where T : struct, Enum
    {
        if (values.Length == 0)
        {
            return string.Empty;
        }

        var table = EnumTables.Get<T>();
        var ids = string.Join(", ", values.Select(GetId));

        return $"DELETE FROM {Identifier(table.Name)} WHERE {Identifier(table.IdColumnName)} IN ({ids});";
    }

    /// <summary>
    /// Removes the column's foreign key to the enum table, whatever the constraint is called. Needed
    /// before <see cref="DropTable{T}"/>, which cannot drop a table anything still references.
    /// </summary>
    /// <remarks>
    /// Found by the relationship rather than by name, for the same reason the add guard is: the
    /// constraint may have been created by hand under a name this code cannot predict, so
    /// <see cref="Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder.DropForeignKey"/> - which
    /// needs the exact name - cannot be used. Loops rather than dropping the first match, so that when
    /// this has run no such foreign key is left: stopping at one would leave a second behind and the
    /// table drop after it would fail anyway, which is a harder failure to read than none at all.
    /// </remarks>
    public static string DropForeignKey<T>(string table, string column)
        where T : struct, Enum
    {
        var enumTable = EnumTables.Get<T>();

        // Composed as a literal rather than interpolated into one, so the identifier is escaped for
        // being inside a string as well as for being an identifier.
        var alterStatement = Literal($"ALTER TABLE {Identifier(table)} DROP CONSTRAINT ");

        return Join(
            "DECLARE @name sysname;",
            "",
            "WHILE 1 = 1",
            "BEGIN",
            "    SET @name = (",
            "        SELECT TOP 1 fk.name",
            ForeignKeyMatch("        ", table, enumTable.Name, column) + ");",
            "",
            "    IF @name IS NULL BREAK;",
            "",
            $"    EXEC({alterStatement} + QUOTENAME(@name));",
            "END");
    }

    public static string DropTable<T>()
        where T : struct, Enum
        => $"DROP TABLE IF EXISTS {Identifier(EnumTables.Get<T>().Name)};";

    /// <summary>
    /// Inserts the given values, skipping any whose id is already present. An existing row is left
    /// alone rather than having its name refreshed: renaming a value is a separate decision, and
    /// doing it here would silently rewrite rows a migration never mentioned.
    /// </summary>
    public static string Insert<T>(params T[] values)
        where T : struct, Enum
    {
        if (values.Length == 0)
        {
            return string.Empty;
        }

        var table = EnumTables.Get<T>();

        var statements = values.Select(value =>
        {
            var id = GetId(value);
            var name = EnumUtils.GetDisplayValue(value);

            return Join(
                $"IF NOT EXISTS (SELECT 1 FROM {Identifier(table.Name)} WHERE {Identifier(table.IdColumnName)} = {id})",
                $"    INSERT INTO {Identifier(table.Name)} ({Identifier(table.IdColumnName)}, [Name]) VALUES ({id}, {Literal(name)});");
        });

        return Join(statements.ToArray());
    }

    /// <summary>
    /// Inserts every value except zero. Zero is the reserved <c>None</c> sentinel - an unset value
    /// rather than a real one - so it is deliberately not a valid foreign key target. Pass it to
    /// <see cref="Insert"/> explicitly if a column genuinely needs to store it.
    /// </summary>
    public static string InsertAll<T>()
        where T : struct, Enum
        => Insert(Enum.GetValues<T>().Where(x => GetId(x) != 0).ToArray());

    /* Shared by the add guard and the drop, so the two cannot come to disagree about what counts as
       "this column's foreign key to the enum table". Indented by the caller because the two nest it at
       different depths. */
    private static string ForeignKeyMatch(string indent, string table, string enumTableName, string column)
        => Join(
            $"{indent}FROM sys.foreign_keys fk",
            $"{indent}INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id",
            $"{indent}WHERE fk.parent_object_id = OBJECT_ID({Literal(table)})",
            $"{indent}    AND fk.referenced_object_id = OBJECT_ID({Literal(enumTableName)})",
            $"{indent}    AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = {Literal(column)}");

    private static int GetId<T>(T value)
        where T : struct, Enum
        => Convert.ToInt32(value);

    private static string Identifier(string name) => $"[{name.Replace("]", "]]")}]";

    private static string Join(params string[] lines) => string.Join(Environment.NewLine, lines);

    private static string Literal(string value) => $"N'{value.Replace("'", "''")}'";
}
