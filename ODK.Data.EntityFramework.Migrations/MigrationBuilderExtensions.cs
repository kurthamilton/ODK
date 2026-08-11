using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Emails;

namespace ODK.Data.EntityFramework.Migrations;

internal static class MigrationBuilderExtensions
{
    private static readonly string[] EmailColumns = ["EmailTypeId", "Subject", "Body", "Overridable", "Name"];

    private static readonly string[] EmailColumnTypes = ["int", "nvarchar(255)", "nvarchar(max)", "bit", "nvarchar(255)"];

    internal static MigrationBuilder DropConstraintIfExists(
        this MigrationBuilder migrationBuilder, string table, string constraint)
    {
        var sql = $"ALTER TABLE [{table}] DROP CONSTRAINT IF EXISTS [{constraint}];";
        migrationBuilder.Sql(sql);
        return migrationBuilder;
    }

    internal static MigrationBuilder InsertEmails(this MigrationBuilder migrationBuilder, params Email[] emails)
    {
        foreach (var email in emails)
        {
            migrationBuilder.InsertData(
                table: "Emails",
                columns: EmailColumns,
                columnTypes: EmailColumnTypes,
                values: new object[,]
                {
                    {
                        (int)email.Type,
                        email.Subject,
                        email.HtmlContent,
                        email.Overridable,
                        email.Type.ToString()
                    }
                });
        }

        return migrationBuilder;
    }
}
