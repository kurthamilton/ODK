namespace ODK.Data.EntityFramework.Migrations;

/// <summary>
/// Builds raw SQL for foreign keys matched by the column they sit on rather than by name.
/// </summary>
/// <remarks>
/// <para>
/// The scaffolder names constraints by EF's convention, and <c>MigrationBuilder.DropForeignKey</c> needs the
/// exact name, so a constraint created by hand under any other name cannot be dropped through it - and the
/// drop fails the whole migration. Much of this schema pre-dates EF and does not follow the convention, and
/// the scaffolder has no way to know: it only ever emits the name it would have chosen itself.
/// </para>
/// <para>
/// Matching on the column drops whatever is actually there. EF then adds its own back under the conventional
/// name, so each table converges on the convention as it is migrated, and the migration never has to name a
/// constraint it did not create.
/// </para>
/// <para>
/// <see cref="Enums.EnumTableSql.DropForeignKey{T}"/> does the same for the enum lookup tables, additionally
/// matching the referenced table. Worth merging the two if a third caller appears.
/// </para>
/// </remarks>
public static class ForeignKeySql
{
    /// <summary>
    /// Drops every foreign key on <paramref name="column"/>, whatever each one is called.
    /// </summary>
    public static string Drop(string table, string column)
    {
        // Composed as a literal rather than interpolated into one, so the identifier is escaped for being
        // inside a string as well as for being an identifier.
        var alterStatement = Literal($"ALTER TABLE {Identifier(table)} DROP CONSTRAINT ");

        /* Variables named for what they act on, because they are batch-scoped: a migration that drops keys
           from several tables emits one of these blocks per table, and the generated script runs them all in
           one batch, where a second DECLARE of the same name is an error. */
        var name = $"@name_{Suffix(table)}_{Suffix(column)}";
        var statement = $"@sql_{Suffix(table)}_{Suffix(column)}";

        /* Loops rather than dropping the first match, so that when this has run no foreign key on the column
           is left: stopping at one would leave a second behind, and whatever the migration does next would
           fail on it - a harder failure to read than none at all. */
        return Join(
            $"DECLARE {name} sysname;",
            $"DECLARE {statement} nvarchar(max);",
            "",
            "WHILE 1 = 1",
            "BEGIN",
            $"    SET {name} = (",
            "        SELECT TOP 1 fk.name",
            "        FROM sys.foreign_keys fk",
            "        INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id",
            $"        WHERE fk.parent_object_id = OBJECT_ID({Literal(table)})",
            $"            AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = {Literal(column)});",
            "",
            $"    IF {name} IS NULL BREAK;",
            "",
            /* Built into a variable and then executed, rather than executed as one expression: EXEC takes
               string literals and variables joined by +, and nothing else, so calling QUOTENAME inside it is
               a syntax error. */
            $"    SET {statement} = {alterStatement} + QUOTENAME({name});",
            $"    EXEC({statement});",
            "END");
    }

    private static string Identifier(string name) => $"[{name.Replace("]", "]]")}]";

    private static string Join(params string[] lines) => string.Join(Environment.NewLine, lines);

    private static string Literal(string value) => $"N'{value.Replace("'", "''")}'";

    // Anything that is not legal in a variable name goes, so an unusual table or column name cannot produce
    // one that will not parse. Only uniqueness within the batch matters, not readability.
    private static string Suffix(string name)
        => new([.. name.Where(x => char.IsLetterOrDigit(x) || x == '_')]);
}
