using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <summary>
    /// Drops the columns the chapter and event tables were keyed on before
    /// ChapterAndEventTables-Id-MakePrimaryKey moved them onto Id. Nothing has read or written them since the
    /// build that migration shipped with.
    /// </summary>
    /// <remarks>
    /// Written by hand: the mapping stopped naming these columns two migrations ago, so the model snapshot
    /// has not known about them since, and EF scaffolds an empty migration. The same goes for the last phase
    /// of every batch.
    /// </remarks>
    public partial class ChapterAndEventTablesOldIdColumnsRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VenueId",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "NewChapterTopicId",
                table: "NewChapterTopics");

            migrationBuilder.DropColumn(
                name: "EventWaitlistMemberId",
                table: "EventWaitlistMembers");

            migrationBuilder.DropColumn(
                name: "EventTicketPaymentId",
                table: "EventTicketPayments");

            migrationBuilder.DropColumn(
                name: "EventHostId",
                table: "EventHosts");

            migrationBuilder.DropColumn(
                name: "EventEmailId",
                table: "EventEmails");

            migrationBuilder.DropColumn(
                name: "EventCommentId",
                table: "EventComments");

            migrationBuilder.DropColumn(
                name: "ChapterQuestionId",
                table: "ChapterQuestions");

            migrationBuilder.DropColumn(
                name: "ChapterPropertyOptionId",
                table: "ChapterPropertyOptions");

            migrationBuilder.DropColumn(
                name: "ChapterPropertyId",
                table: "ChapterProperties");

            migrationBuilder.DropColumn(
                name: "ChapterPageId",
                table: "ChapterPages");

            migrationBuilder.DropColumn(
                name: "ChapterEmailId",
                table: "ChapterEmails");

            migrationBuilder.DropColumn(
                name: "ChapterConversationId",
                table: "ChapterConversations");

            migrationBuilder.DropColumn(
                name: "ChapterConversationMessageId",
                table: "ChapterConversationMessages");

            migrationBuilder.DropColumn(
                name: "ChapterContactMessageId",
                table: "ChapterContactMessages");

            migrationBuilder.DropColumn(
                name: "ChapterContactMessageReplyId",
                table: "ChapterContactMessageReplies");

            migrationBuilder.DropColumn(
                name: "ChapterAdminMemberId",
                table: "ChapterAdminMembers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /* Nullable, and only the data restored: these columns were the key when this migration's Up ran,
               but putting that back is the previous migration's Down, not this one's. Going back through both
               returns the tables to where they started, and nothing is lost either way - the values are Id's. */
            migrationBuilder.AddColumn<Guid>(
                name: "VenueId",
                table: "Venues",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NewChapterTopicId",
                table: "NewChapterTopics",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EventWaitlistMemberId",
                table: "EventWaitlistMembers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EventTicketPaymentId",
                table: "EventTicketPayments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EventHostId",
                table: "EventHosts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EventEmailId",
                table: "EventEmails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EventCommentId",
                table: "EventComments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ChapterQuestionId",
                table: "ChapterQuestions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ChapterPropertyOptionId",
                table: "ChapterPropertyOptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ChapterPropertyId",
                table: "ChapterProperties",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ChapterPageId",
                table: "ChapterPages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ChapterEmailId",
                table: "ChapterEmails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ChapterConversationId",
                table: "ChapterConversations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ChapterConversationMessageId",
                table: "ChapterConversationMessages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ChapterContactMessageId",
                table: "ChapterContactMessages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ChapterContactMessageReplyId",
                table: "ChapterContactMessageReplies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ChapterAdminMemberId",
                table: "ChapterAdminMembers",
                type: "uniqueidentifier",
                nullable: true);

            // EXEC because the generated script puts these in the same batch as the columns they write to,
            // and SQL Server binds column names when it parses a batch - see LookupTables-Id-Add.
            migrationBuilder.Sql("EXEC('UPDATE Venues SET VenueId = Id WHERE VenueId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE NewChapterTopics SET NewChapterTopicId = Id WHERE NewChapterTopicId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE EventWaitlistMembers SET EventWaitlistMemberId = Id WHERE EventWaitlistMemberId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE EventTicketPayments SET EventTicketPaymentId = Id WHERE EventTicketPaymentId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE EventHosts SET EventHostId = Id WHERE EventHostId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE EventEmails SET EventEmailId = Id WHERE EventEmailId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE EventComments SET EventCommentId = Id WHERE EventCommentId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ChapterQuestions SET ChapterQuestionId = Id WHERE ChapterQuestionId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ChapterPropertyOptions SET ChapterPropertyOptionId = Id WHERE ChapterPropertyOptionId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ChapterProperties SET ChapterPropertyId = Id WHERE ChapterPropertyId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ChapterPages SET ChapterPageId = Id WHERE ChapterPageId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ChapterEmails SET ChapterEmailId = Id WHERE ChapterEmailId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ChapterConversations SET ChapterConversationId = Id WHERE ChapterConversationId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ChapterConversationMessages SET ChapterConversationMessageId = Id WHERE ChapterConversationMessageId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ChapterContactMessages SET ChapterContactMessageId = Id WHERE ChapterContactMessageId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ChapterContactMessageReplies SET ChapterContactMessageReplyId = Id WHERE ChapterContactMessageReplyId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ChapterAdminMembers SET ChapterAdminMemberId = Id WHERE ChapterAdminMemberId IS NULL')");
        }
    }
}
