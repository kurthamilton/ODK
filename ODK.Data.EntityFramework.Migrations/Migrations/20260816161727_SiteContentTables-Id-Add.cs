using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class SiteContentTablesIdAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "SiteQuestions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "SiteContactMessages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "SiteContactMessageReplies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Issues",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "IssueMessages",
                type: "uniqueidentifier",
                nullable: true);

            /* Copy the key across. Nullable for now and left that way deliberately: the build running while
               this is applied knows only the old column, so a row it inserts arrives with Id unset. The
               migration that makes Id the key backfills again before it adds the constraint.

               WHERE Id IS NULL so re-running fills only what is unset rather than rewriting every row.

               EXEC because the generated script puts the ALTER above and the UPDATE in one batch, and SQL
               Server binds column names when it parses a batch - so a plain UPDATE fails with "Invalid column
               name 'Id'" against a column the same batch is adding. */
            migrationBuilder.Sql("EXEC('UPDATE SiteQuestions SET Id = SiteQuestionId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE SiteContactMessages SET Id = SiteContactMessageId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE SiteContactMessageReplies SET Id = SiteContactMessageReplyId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Issues SET Id = IssueId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE IssueMessages SET Id = IssueMessageId WHERE Id IS NULL')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "SiteQuestions");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "SiteContactMessages");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "SiteContactMessageReplies");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "IssueMessages");
        }
    }
}
