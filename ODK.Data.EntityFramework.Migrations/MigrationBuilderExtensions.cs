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
