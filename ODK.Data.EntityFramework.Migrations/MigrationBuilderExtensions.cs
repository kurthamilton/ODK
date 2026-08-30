using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Emails;

namespace ODK.Data.EntityFramework.Migrations;

internal static class MigrationBuilderExtensions
{
    /* A migration inserts with the columns the table had when the migration was written, so each schema
       era keeps its own set. Do not merge them: a migration that runs before a column exists must insert
       without it, or a database built from the migrations alone fails on that migration. */
    private static readonly string[] IdKeyColumns =
        ["Id", "Subject", "Body", "Overridable", "Name", "EmailRecipientTypeId"];

    private static readonly string[] IdKeyColumnTypes =
        ["int", "nvarchar(255)", "nvarchar(max)", "bit", "nvarchar(255)", "int"];

    private static readonly string[] TypeIdKeyColumns = ["EmailTypeId", "Subject", "Body", "Overridable", "Name"];

    private static readonly string[] TypeIdKeyColumnTypes =
        ["int", "nvarchar(255)", "nvarchar(max)", "bit", "nvarchar(255)"];

    private static readonly string[] TypeIdKeyWithRecipientTypeColumns = [.. TypeIdKeyColumns, "EmailRecipientTypeId"];

    private static readonly string[] TypeIdKeyWithRecipientTypeColumnTypes = [.. TypeIdKeyColumnTypes, "int"];

    internal static MigrationBuilder DeleteEmail(
        this MigrationBuilder migrationBuilder, EmailSchemaEra era, EmailType type)
    {
        migrationBuilder.DeleteData(
            table: "Emails",
            keyColumn: KeyColumn(era),
            keyValue: (int)type);
        return migrationBuilder;
    }

    /// <summary>
    /// Drops a column where the table has one, and its default constraint with it - use in place of
    /// <see cref="MigrationBuilder.DropColumn"/> for a column no migration created, which a database built from
    /// the migrations alone will not have. Any index or foreign key on the column is the caller's to clear
    /// first. See <see cref="ColumnSql.Drop"/>.
    /// </summary>
    internal static MigrationBuilder DropColumnIfExists(
        this MigrationBuilder migrationBuilder, string table, string column)
    {
        migrationBuilder.Sql(ColumnSql.Drop(table, column));
        return migrationBuilder;
    }

    internal static MigrationBuilder DropConstraintIfExists(
        this MigrationBuilder migrationBuilder, string table, string name)
    {
        var sql = $"ALTER TABLE [{table}] DROP CONSTRAINT IF EXISTS [{name}];";
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
        this MigrationBuilder migrationBuilder, string table, string name)
    {
        migrationBuilder.Sql($"DROP INDEX IF EXISTS [{name}] ON [{table}];");
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
    /// Inserts rows into Emails using the column set for the given <see cref="EmailSchemaEra"/>.
    /// </summary>
    /// <remarks>
    /// The era is the caller's to state, because nothing about an <see cref="Email"/> reveals it: the key
    /// column moved from EmailTypeId to Id, and a row carries no trace of which schema it is being written
    /// into. Pass the era the migration was written for and never change it - see
    /// <see cref="EmailSchemaEra"/>.
    /// </remarks>
    internal static MigrationBuilder InsertEmails(
        this MigrationBuilder migrationBuilder, EmailSchemaEra era, params Email[] emails)
    {
        var (columns, columnTypes) = Columns(era);

        foreach (var email in emails)
        {
            object[] values =
            [
                (int)email.Type,
                email.Subject,
                email.BodyHtml,
                email.IsGroupEmail,
                email.Type.ToString()
            ];

            /* The recipient type is the only column any era adds, so the one era without it is the one era
               that writes the shorter value list. */
            migrationBuilder.InsertData(
                table: "Emails",
                columns: columns,
                columnTypes: columnTypes,
                values: era == EmailSchemaEra.TypeIdKey ? values : [.. values, (int)email.RecipientType]);
        }

        return migrationBuilder;
    }

    /// <summary>
    /// Rewrites the site's wording for an email, keyed by the column the given <see cref="EmailSchemaEra"/>
    /// keys on - the key column has moved, so an update has to state its era for the same reason an insert
    /// does.
    /// </summary>
    /// <remarks>
    /// A group's own override lives in ChapterEmails and is deliberately left alone: a group that customised
    /// this template chose its wording, and the site's is only what the rest of them inherit.
    /// </remarks>
    internal static MigrationBuilder UpdateEmailWording(
        this MigrationBuilder migrationBuilder,
        EmailSchemaEra era,
        EmailType type,
        string subject,
        string body)
    {
        migrationBuilder.UpdateData(
            table: "Emails",
            keyColumn: KeyColumn(era),
            keyValue: (int)type,
            columns: ["Subject", "Body"],
            values: [subject, body]);
        return migrationBuilder;
    }

    private static (string[] Columns, string[] ColumnTypes) Columns(EmailSchemaEra era) => era switch
    {
        EmailSchemaEra.TypeIdKey => (TypeIdKeyColumns, TypeIdKeyColumnTypes),
        EmailSchemaEra.TypeIdKeyWithRecipientType =>
            (TypeIdKeyWithRecipientTypeColumns, TypeIdKeyWithRecipientTypeColumnTypes),
        EmailSchemaEra.IdKey => (IdKeyColumns, IdKeyColumnTypes),
        _ => throw new ArgumentOutOfRangeException(nameof(era), era, "No Emails column set for this era.")
    };

    private static string KeyColumn(EmailSchemaEra era) => era switch
    {
        EmailSchemaEra.TypeIdKey or EmailSchemaEra.TypeIdKeyWithRecipientType => "EmailTypeId",
        EmailSchemaEra.IdKey => "Id",
        _ => throw new ArgumentOutOfRangeException(nameof(era), era, "No Emails key column for this era.")
    };
}
