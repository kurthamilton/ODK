namespace ODK.Data.EntityFramework.Migrations;

/// <summary>
/// Builds raw SQL for indexes matched by the column they cover rather than by name.
/// </summary>
/// <remarks>
/// <para>
/// The counterpart to <see cref="ForeignKeySql"/>, and for the same reason: where the database already has
/// an index EF does not know about, adding the relationship to the model scaffolds a <c>CreateIndex</c> with
/// nothing to drop first. If the existing index happens to carry EF's name the create fails; if it carries
/// any other name the create succeeds and leaves two indexes covering one column.
/// </para>
/// <para>
/// Deliberately narrow. Only an index that duplicates what EF is about to create is dropped: non-unique,
/// nonclustered, and keyed on that one column alone. A composite index that merely begins with the column
/// serves queries this knows nothing about, a unique one is a constraint rather than a lookup aid, and
/// dropping a clustered one would rewrite the table - none of those are duplicates, so none are touched.
/// </para>
/// <para>
/// The small string helpers below are copied from <see cref="ForeignKeySql"/> rather than shared, matching
/// how <see cref="Enums.EnumTableSql"/> keeps its own. Worth collapsing into one place if a fourth appears.
/// </para>
/// </remarks>
public static class IndexSql
{
    /// <summary>
    /// Drops every index that duplicates a plain single-column index on <paramref name="column"/>.
    /// </summary>
    public static string Drop(string table, string column)
    {
        // Composed as a literal rather than interpolated into one, so the identifier is escaped for being
        // inside a string as well as for being an identifier.
        var onTable = Literal($" ON {Identifier(table)}");

        // Named for what they act on, because variables are scoped to the batch rather than the block.
        var name = $"@index_{Suffix(table)}_{Suffix(column)}";
        var statement = $"@sqlix_{Suffix(table)}_{Suffix(column)}";

        return Join(
            $"DECLARE {name} sysname;",
            $"DECLARE {statement} nvarchar(max);",
            "",
            "WHILE 1 = 1",
            "BEGIN",
            $"    SET {name} = (",
            "        SELECT TOP 1 i.name",
            "        FROM sys.indexes i",
            $"        WHERE i.object_id = OBJECT_ID({Literal(table)})",
            "            AND i.is_primary_key = 0",
            "            AND i.is_unique = 0",
            "            AND i.type_desc = N'NONCLUSTERED'",
            // Keyed on this column and nothing else - included columns do not count towards the key.
            "            AND (",
            "                SELECT COUNT(*)",
            "                FROM sys.index_columns ic",
            "                WHERE ic.object_id = i.object_id",
            "                    AND ic.index_id = i.index_id",
            "                    AND ic.is_included_column = 0) = 1",
            "            AND EXISTS (",
            "                SELECT 1",
            "                FROM sys.index_columns ic",
            "                WHERE ic.object_id = i.object_id",
            "                    AND ic.index_id = i.index_id",
            "                    AND ic.is_included_column = 0",
            $"                    AND COL_NAME(ic.object_id, ic.column_id) = {Literal(column)}));",
            "",
            $"    IF {name} IS NULL BREAK;",
            "",
            // Built into a variable and then executed: EXEC takes string literals and variables joined by +,
            // so calling QUOTENAME inside it is a syntax error.
            $"    SET {statement} = N'DROP INDEX ' + QUOTENAME({name}) + {onTable};",
            $"    EXEC({statement});",
            "END");
    }

    private static string Identifier(string name) => $"[{name.Replace("]", "]]")}]";

    private static string Join(params string[] lines) => string.Join(Environment.NewLine, lines);

    private static string Literal(string value) => $"N'{value.Replace("'", "''")}'";

    private static string Suffix(string name)
        => new([.. name.Where(x => char.IsLetterOrDigit(x) || x == '_')]);
}
