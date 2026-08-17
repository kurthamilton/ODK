using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Emails;

namespace ODK.Data.EntityFramework.Migrations;

internal static class MigrationBuilderExtensions
{
    /* A migration inserts with the columns the table had when the migration was written, so each schema
       era keeps its own set. Do not merge them: a migration that runs before a column exists must insert
       without it, or a database built from the migrations alone fails on that migration. */
    private static readonly string[] EmailColumns = ["EmailTypeId", "Subject", "Body", "Overridable", "Name"];

    private static readonly string[] EmailColumnsWithRecipientType = [.. EmailColumns, "EmailRecipientTypeId"];

    private static readonly string[] EmailColumnTypes = ["int", "nvarchar(255)", "nvarchar(max)", "bit", "nvarchar(255)"];

    private static readonly string[] EmailColumnTypesWithRecipientType = [.. EmailColumnTypes, "int"];

    internal static MigrationBuilder DeleteEmail(this MigrationBuilder migrationBuilder, EmailType type)
    {
        migrationBuilder.DeleteData(
            table: "Emails",
            keyColumn: "EmailTypeId",
            keyValue: (int)type);
        return migrationBuilder;
    }

    /// <summary>
    /// Drops a column where the table has one - use in place of <see cref="MigrationBuilder.DropColumn"/> for
    /// a column no migration created, which a database built from the migrations alone will not have.
    /// </summary>
    internal static MigrationBuilder DropColumnIfExists(
        this MigrationBuilder migrationBuilder, string table, string column)
    {
        var sql =
            $"IF COL_LENGTH(N'{table}', N'{column}') IS NOT NULL" + Environment.NewLine +
            $"    ALTER TABLE [{table}] DROP COLUMN [{column}];";
        migrationBuilder.Sql(sql);
        return migrationBuilder;
    }

    internal static MigrationBuilder DropConstraintIfExists(
        this MigrationBuilder migrationBuilder, string table, string constraint)
    {
        var sql = $"ALTER TABLE [{table}] DROP CONSTRAINT IF EXISTS [{constraint}];";
        migrationBuilder.Sql(sql);
        return migrationBuilder;
    }

    /// <summary>
    /// Drops every foreign key on a column, whatever each one is called - use in place of
    /// <see cref="MigrationBuilder.DropForeignKey"/>, which needs a name the scaffolder can only guess.
    /// See <see cref="ForeignKeySql.Drop"/>.
    /// </summary>
    internal static MigrationBuilder DropForeignKeys(
        this MigrationBuilder migrationBuilder, string table, string column)
    {
        migrationBuilder.Sql(ForeignKeySql.Drop(table, column));
        return migrationBuilder;
    }

    /// <summary>
    /// Drops any existing plain index on a column, whatever it is called, so a scaffolded
    /// <see cref="MigrationBuilder.CreateIndex"/> for that column neither collides with it nor leaves a
    /// duplicate beside it. See <see cref="IndexSql.Drop"/>.
    /// </summary>
    internal static MigrationBuilder DropIndexes(
        this MigrationBuilder migrationBuilder, string table, string column)
    {
        migrationBuilder.Sql(IndexSql.Drop(table, column));
        return migrationBuilder;
    }

    /// <summary>
    /// Drops an index by name where it exists. Use when clearing the way for a column drop, which any index
    /// on the column blocks - <see cref="DropIndexes"/> is scoped to one duplicating an index EF is about to
    /// create, and passes over a unique or clustered one.
    /// </summary>
    internal static MigrationBuilder DropIndexIfExists(
        this MigrationBuilder migrationBuilder, string table, string index)
    {
        migrationBuilder.Sql($"DROP INDEX IF EXISTS [{index}] ON [{table}];");
        return migrationBuilder;
    }

    /// <summary>
    /// Drops a table's primary key, whatever it is called, and does nothing where the table has none - use
    /// in place of <see cref="MigrationBuilder.DropPrimaryKey"/>, which needs the exact name and assumes the
    /// constraint exists. See <see cref="PrimaryKeySql.Drop"/>.
    /// </summary>
    internal static MigrationBuilder DropPrimaryKeyIfExists(
        this MigrationBuilder migrationBuilder, string table)
    {
        migrationBuilder.Sql(PrimaryKeySql.Drop(table));
        return migrationBuilder;
    }

    /// <summary>
    /// Drops a table where the database has one - use in place of <see cref="MigrationBuilder.DropTable"/> for
    /// a table no migration created, which a database built from the migrations alone will not have.
    /// </summary>
    internal static MigrationBuilder DropTableIfExists(this MigrationBuilder migrationBuilder, string table)
    {
        migrationBuilder.Sql($"DROP TABLE IF EXISTS [{table}];");
        return migrationBuilder;
    }

    /// <summary>
    /// Inserts rows into Emails, writing EmailRecipientTypeId for each email that sets
    /// <see cref="Email.RecipientType"/> and omitting the column for each one that leaves it unset.
    /// </summary>
    /// <remarks>
    /// The recipient type is what selects the schema era, so a migration gets the right one from the rows
    /// it declares and has no separate switch to get wrong. Leaving it unset targets the schema without
    /// EmailRecipientTypeId, which is the era a migration older than that column has to keep inserting
    /// for. A newer migration that omits it inserts nothing into a NOT NULL column with no default and
    /// fails there, which is the wanted outcome: <see cref="EmailRecipientType.None"/> has no row in the
    /// lookup table, so there is no such thing as a valid row carrying it.
    /// </remarks>
    internal static MigrationBuilder InsertEmails(this MigrationBuilder migrationBuilder, params Email[] emails)
    {
        foreach (var email in emails)
        {
            var withRecipientType = email.RecipientType != EmailRecipientType.None;

            object[] values =
            [
                (int)email.Type,
                email.Subject,
                email.HtmlContent,
                email.Overridable,
                email.Type.ToString()
            ];

            // Columns, types and values all follow the one condition, so they cannot come to disagree.
            migrationBuilder.InsertData(
                table: "Emails",
                columns: withRecipientType ? EmailColumnsWithRecipientType : EmailColumns,
                columnTypes: withRecipientType ? EmailColumnTypesWithRecipientType : EmailColumnTypes,
                values: withRecipientType ? [.. values, (int)email.RecipientType] : values);
        }

        return migrationBuilder;
    }
}
