namespace ODK.Data.EntityFramework.Migrations;

/// <summary>
/// Builds raw SQL for dropping a column the model may never have declared, clearing the default constraint
/// that would otherwise block it.
/// </summary>
/// <remarks>
/// <para>
/// <c>MigrationBuilder.DropColumn</c> covers the default constraint already, but only for a column EF knows
/// about - and a column EF knows about is one every database has, since a migration created it. The case this
/// exists for is the opposite: a column that only a restored database has, where the unguarded drop fails
/// against a database built from the migrations alone. Reach for <c>DropColumn</c> whenever the column came
/// from a migration; this is for the ones that did not.
/// </para>
/// <para>
/// A default constraint is named by whoever created it, so like the rest of this set it is found by what it
/// belongs to rather than by a name the scaffolder could only guess.
/// </para>
/// <para>
/// Indexes and foreign keys on the column are the caller's to clear first - see
/// <see cref="ForeignKeySql.Drop"/> and the named <c>DropIndexIfExists</c>. They are deliberately not done
/// here: dropping every index covering a column is a broader act than dropping the column, and a caller that
/// has not thought about which indexes those are should be stopped by the error rather than have them removed
/// silently.
/// </para>
/// </remarks>
public static class ColumnSql
{
    /// <summary>
    /// Drops <paramref name="column"/> from <paramref name="table"/> where the table has it, first dropping its
    /// default constraint, whatever that is called.
    /// </summary>
    public static string Drop(string table, string column)
    {
        // Composed as a literal rather than interpolated into one, so the identifier is escaped for being
        // inside a string as well as for being an identifier.
        var alterStatement = Literal($"ALTER TABLE {Identifier(table)} DROP CONSTRAINT ");

        /* Named for what they act on, because variables are scoped to the batch rather than the block: a
           migration dropping several columns emits one of these per column, all into one batch. */
        var name = $"@default_{Suffix(table)}_{Suffix(column)}";
        var statement = $"@sqldefault_{Suffix(table)}_{Suffix(column)}";

        return Join(
            $"DECLARE {name} sysname;",
            $"DECLARE {statement} nvarchar(max);",
            "",
            $"SET {name} = (",
            "    SELECT TOP 1 dc.name",
            "    FROM sys.default_constraints dc",
            "    INNER JOIN sys.columns c ON c.object_id = dc.parent_object_id",
            "        AND c.column_id = dc.parent_column_id",
            $"    WHERE dc.parent_object_id = OBJECT_ID({Literal(table)})",
            $"        AND c.name = {Literal(column)});",
            "",
            $"IF {name} IS NOT NULL",
            "BEGIN",
            // Built into a variable and then executed: EXEC takes string literals and variables joined by +,
            // so calling QUOTENAME inside it is a syntax error.
            $"    SET {statement} = {alterStatement} + QUOTENAME({name});",
            $"    EXEC({statement});",
            "END",
            "",
            $"IF COL_LENGTH({Literal(table)}, {Literal(column)}) IS NOT NULL",
            $"    ALTER TABLE {Identifier(table)} DROP COLUMN {Identifier(column)};");
    }

    private static string Identifier(string name) => $"[{name.Replace("]", "]]")}]";

    private static string Join(params string[] lines) => string.Join(Environment.NewLine, lines);

    private static string Literal(string value) => $"N'{value.Replace("'", "''")}'";

    private static string Suffix(string name)
        => new([.. name.Where(x => char.IsLetterOrDigit(x) || x == '_')]);
}
