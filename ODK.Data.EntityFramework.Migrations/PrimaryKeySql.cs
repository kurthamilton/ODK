namespace ODK.Data.EntityFramework.Migrations;

/// <summary>
/// Builds raw SQL for a primary key found by the table it belongs to rather than by name.
/// </summary>
/// <remarks>
/// <para>
/// The third of the set, alongside <see cref="ForeignKeySql"/> and <see cref="IndexSql"/>, and it covers two
/// failures at once. <c>MigrationBuilder.DropPrimaryKey</c> needs the constraint's exact name, which the
/// scaffolder can only guess from EF's convention - and worse, it assumes there is one at all. The model
/// declaring a key does not mean the database has the constraint: <c>SentEmailEvents</c> was mapped with
/// <c>HasKey</c> for years while the table had no primary key, and nothing said so until a migration tried
/// to drop it.
/// </para>
/// <para>
/// Looking the name up handles both - a key under an unexpected name is still found, and a table without one
/// is left alone instead of failing the migration.
/// </para>
/// <para>
/// No loop, unlike the other two: a table has at most one primary key.
/// </para>
/// </remarks>
public static class PrimaryKeySql
{
    /// <summary>
    /// Drops <paramref name="table"/>'s primary key, whatever it is called, and does nothing where the table
    /// has none.
    /// </summary>
    public static string Drop(string table)
    {
        // Composed as a literal rather than interpolated into one, so the identifier is escaped for being
        // inside a string as well as for being an identifier.
        var alterStatement = Literal($"ALTER TABLE {Identifier(table)} DROP CONSTRAINT ");

        // Named for what it acts on, because variables are scoped to the batch rather than the block.
        var name = $"@pk_{Suffix(table)}";
        var statement = $"@sqlpk_{Suffix(table)}";

        return Join(
            $"DECLARE {name} sysname;",
            $"DECLARE {statement} nvarchar(max);",
            "",
            $"SET {name} = (",
            "    SELECT TOP 1 kc.name",
            "    FROM sys.key_constraints kc",
            $"    WHERE kc.parent_object_id = OBJECT_ID({Literal(table)})",
            "        AND kc.type = 'PK');",
            "",
            $"IF {name} IS NOT NULL",
            "BEGIN",
            // Built into a variable and then executed: EXEC takes string literals and variables joined by +,
            // so calling QUOTENAME inside it is a syntax error.
            $"    SET {statement} = {alterStatement} + QUOTENAME({name});",
            $"    EXEC({statement});",
            "END");
    }

    private static string Identifier(string name) => $"[{name.Replace("]", "]]")}]";

    private static string Join(params string[] lines) => string.Join(Environment.NewLine, lines);

    private static string Literal(string value) => $"N'{value.Replace("'", "''")}'";

    private static string Suffix(string name)
        => new([.. name.Where(x => char.IsLetterOrDigit(x) || x == '_')]);
}
