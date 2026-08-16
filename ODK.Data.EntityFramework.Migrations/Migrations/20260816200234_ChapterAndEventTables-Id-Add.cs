using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ChapterAndEventTablesIdAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Venues",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "NewChapterTopics",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "EventWaitlistMembers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "EventTicketPayments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "EventHosts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "EventEmails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "EventComments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ChapterQuestions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ChapterPropertyOptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ChapterProperties",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ChapterPages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ChapterEmails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ChapterConversations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ChapterConversationMessages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ChapterContactMessages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ChapterContactMessageReplies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ChapterAdminMembers",
                type: "uniqueidentifier",
                nullable: true);

            /* Copy the key across. Nullable for now and left that way deliberately: the build running while
               this is applied knows only the old column, so a row it inserts arrives with Id unset. The
               migration that makes Id the key backfills again before it adds the constraint.

               EXEC because the generated script puts the ALTER above and the UPDATE in one batch, and SQL
               Server binds column names when it parses a batch - see LookupTables-Id-Add. */
            migrationBuilder.Sql("EXEC('UPDATE Venues SET Id = VenueId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE NewChapterTopics SET Id = NewChapterTopicId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE EventWaitlistMembers SET Id = EventWaitlistMemberId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE EventTicketPayments SET Id = EventTicketPaymentId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE EventHosts SET Id = EventHostId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE EventEmails SET Id = EventEmailId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE EventComments SET Id = EventCommentId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ChapterQuestions SET Id = ChapterQuestionId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ChapterPropertyOptions SET Id = ChapterPropertyOptionId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ChapterProperties SET Id = ChapterPropertyId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ChapterPages SET Id = ChapterPageId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ChapterEmails SET Id = ChapterEmailId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ChapterConversations SET Id = ChapterConversationId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ChapterConversationMessages SET Id = ChapterConversationMessageId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ChapterContactMessages SET Id = ChapterContactMessageId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ChapterContactMessageReplies SET Id = ChapterContactMessageReplyId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ChapterAdminMembers SET Id = ChapterAdminMemberId WHERE Id IS NULL')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "NewChapterTopics");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "EventWaitlistMembers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "EventTicketPayments");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "EventHosts");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "EventEmails");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "EventComments");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ChapterQuestions");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ChapterPropertyOptions");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ChapterProperties");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ChapterPages");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ChapterEmails");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ChapterConversations");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ChapterConversationMessages");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ChapterContactMessages");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ChapterContactMessageReplies");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ChapterAdminMembers");
        }
    }
}
