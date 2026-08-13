using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ChapterEmailsSubjectAndHtmlContentMakeNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "ChapterEmails",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "HtmlContent",
                table: "ChapterEmails",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /* Fill the unset fields from the site's email before the columns stop allowing null. Without
               this the scaffolded default turns each one into an empty string, and a schema that cannot say
               "not overridden" would then send an empty subject or body instead of the site's. Per column:
               a row may override one field and inherit the other. */
            migrationBuilder.Sql(
                """
                UPDATE ce
                SET ce.Subject = e.Subject
                FROM ChapterEmails ce
                INNER JOIN Emails e ON e.EmailTypeId = ce.EmailTypeId
                WHERE ce.Subject IS NULL
                """);

            migrationBuilder.Sql(
                """
                UPDATE ce
                SET ce.HtmlContent = e.Body
                FROM ChapterEmails ce
                INNER JOIN Emails e ON e.EmailTypeId = ce.EmailTypeId
                WHERE ce.HtmlContent IS NULL
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "ChapterEmails",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HtmlContent",
                table: "ChapterEmails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
