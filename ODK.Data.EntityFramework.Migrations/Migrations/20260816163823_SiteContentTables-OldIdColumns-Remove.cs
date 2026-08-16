using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <summary>
    /// Drops the columns the site content tables were keyed on before SiteContentTables-Id-MakePrimaryKey
    /// moved them onto Id. Nothing has read or written them since the build that migration shipped with.
    /// </summary>
    /// <remarks>
    /// Written by hand: the mapping stopped naming these columns two migrations ago, so the model snapshot
    /// has not known about them since, and EF scaffolds an empty migration. The same goes for the last phase
    /// of every batch.
    /// </remarks>
    public partial class SiteContentTablesOldIdColumnsRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SiteQuestionId",
                table: "SiteQuestions");

            migrationBuilder.DropColumn(
                name: "SiteContactMessageId",
                table: "SiteContactMessages");

            migrationBuilder.DropColumn(
                name: "SiteContactMessageReplyId",
                table: "SiteContactMessageReplies");

            migrationBuilder.DropColumn(
                name: "IssueId",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "IssueMessageId",
                table: "IssueMessages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /* Nullable, and only the data restored: the column was the key when this migration's Up ran, but
               putting that back is the previous migration's Down, not this one's. Going back through both
               returns the tables to where they started, and nothing is lost either way - the values are Id's. */
            migrationBuilder.AddColumn<Guid>(
                name: "SiteQuestionId",
                table: "SiteQuestions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SiteContactMessageId",
                table: "SiteContactMessages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SiteContactMessageReplyId",
                table: "SiteContactMessageReplies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IssueId",
                table: "Issues",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IssueMessageId",
                table: "IssueMessages",
                type: "uniqueidentifier",
                nullable: true);

            // EXEC because the generated script puts these in the same batch as the columns they write to,
            // and SQL Server binds column names when it parses a batch - see LookupTables-Id-Add.
            migrationBuilder.Sql("EXEC('UPDATE SiteQuestions SET SiteQuestionId = Id WHERE SiteQuestionId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE SiteContactMessages SET SiteContactMessageId = Id WHERE SiteContactMessageId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE SiteContactMessageReplies SET SiteContactMessageReplyId = Id WHERE SiteContactMessageReplyId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Issues SET IssueId = Id WHERE IssueId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE IssueMessages SET IssueMessageId = Id WHERE IssueMessageId IS NULL')");
        }
    }
}
