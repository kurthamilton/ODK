using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ChapterAndEventTablesIdMakePrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* Fill Id for anything the previous migration missed: it backfilled when it was applied, and
               the build that was live for the rest of that deploy knew only the old column. EXEC because
               the generated script batches statements together - see LookupTables-Id-Add. */
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

            /* Neither of these is scaffolded. The database enforces Events -> Venues and indexes the
               column, but the model only learned of the relationship after both existed, so EF emits the
               constraint and the index to create and nothing to drop. Venues is being re-keyed, so the
               old constraint has to go first, and the index it would create is already there. */
            migrationBuilder.DropForeignKeys("Events", "VenueId");
            migrationBuilder.DropIndexes("Events", "VenueId");
            migrationBuilder.DropForeignKeys("ChapterContactMessageReplies", "ChapterContactMessageId");

            migrationBuilder.DropForeignKeys("ChapterConversationMessages", "ChapterConversationId");

            migrationBuilder.DropForeignKeys("ChapterPropertyOptions", "ChapterPropertyId");

            migrationBuilder.DropForeignKeys("EventComments", "ParentEventCommentId");

            migrationBuilder.DropForeignKeys("MemberProperties", "ChapterPropertyId");

            migrationBuilder.DropForeignKeys("VenueLocations", "VenueId");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Venues",
                table: "Venues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NewChapterTopics",
                table: "NewChapterTopics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventWaitlistMembers",
                table: "EventWaitlistMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventTicketPayments",
                table: "EventTicketPayments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventHosts",
                table: "EventHosts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventEmails",
                table: "EventEmails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventComments",
                table: "EventComments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterQuestions",
                table: "ChapterQuestions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterPropertyOptions",
                table: "ChapterPropertyOptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterProperties",
                table: "ChapterProperties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterPages",
                table: "ChapterPages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterEmails",
                table: "ChapterEmails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterConversations",
                table: "ChapterConversations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterConversationMessages",
                table: "ChapterConversationMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterContactMessages",
                table: "ChapterContactMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterContactMessageReplies",
                table: "ChapterContactMessageReplies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterAdminMembers",
                table: "ChapterAdminMembers");

            migrationBuilder.AlterColumn<Guid>(
                name: "VenueId",
                table: "Venues",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "NewChapterTopicId",
                table: "NewChapterTopics",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "EventWaitlistMemberId",
                table: "EventWaitlistMembers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "EventTicketPaymentId",
                table: "EventTicketPayments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "EventHostId",
                table: "EventHosts",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "EventEmailId",
                table: "EventEmails",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "EventCommentId",
                table: "EventComments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterQuestionId",
                table: "ChapterQuestions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterPropertyOptionId",
                table: "ChapterPropertyOptions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterPropertyId",
                table: "ChapterProperties",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterPageId",
                table: "ChapterPages",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterEmailId",
                table: "ChapterEmails",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterConversationId",
                table: "ChapterConversations",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterConversationMessageId",
                table: "ChapterConversationMessages",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterContactMessageId",
                table: "ChapterContactMessages",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterContactMessageReplyId",
                table: "ChapterContactMessageReplies",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterAdminMemberId",
                table: "ChapterAdminMembers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Venues",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "NewChapterTopics",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "EventWaitlistMembers",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "EventTicketPayments",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "EventHosts",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "EventEmails",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "EventComments",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ChapterQuestions",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ChapterPropertyOptions",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ChapterProperties",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ChapterPages",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ChapterEmails",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ChapterConversations",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ChapterConversationMessages",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ChapterContactMessages",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ChapterContactMessageReplies",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ChapterAdminMembers",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Venues",
                table: "Venues",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NewChapterTopics",
                table: "NewChapterTopics",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventWaitlistMembers",
                table: "EventWaitlistMembers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventTicketPayments",
                table: "EventTicketPayments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventHosts",
                table: "EventHosts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventEmails",
                table: "EventEmails",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventComments",
                table: "EventComments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterQuestions",
                table: "ChapterQuestions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterPropertyOptions",
                table: "ChapterPropertyOptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterProperties",
                table: "ChapterProperties",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterPages",
                table: "ChapterPages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterEmails",
                table: "ChapterEmails",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterConversations",
                table: "ChapterConversations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterConversationMessages",
                table: "ChapterConversationMessages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterContactMessages",
                table: "ChapterContactMessages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterContactMessageReplies",
                table: "ChapterContactMessageReplies",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterAdminMembers",
                table: "ChapterAdminMembers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Events_VenueId",
                table: "Events",
                column: "VenueId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterContactMessageReplies_ChapterContactMessages_ChapterContactMessageId",
                table: "ChapterContactMessageReplies",
                column: "ChapterContactMessageId",
                principalTable: "ChapterContactMessages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterConversationMessages_ChapterConversations_ChapterConversationId",
                table: "ChapterConversationMessages",
                column: "ChapterConversationId",
                principalTable: "ChapterConversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterPropertyOptions_ChapterProperties_ChapterPropertyId",
                table: "ChapterPropertyOptions",
                column: "ChapterPropertyId",
                principalTable: "ChapterProperties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventComments_EventComments_ParentEventCommentId",
                table: "EventComments",
                column: "ParentEventCommentId",
                principalTable: "EventComments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Venues_VenueId",
                table: "Events",
                column: "VenueId",
                principalTable: "Venues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberProperties_ChapterProperties_ChapterPropertyId",
                table: "MemberProperties",
                column: "ChapterPropertyId",
                principalTable: "ChapterProperties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VenueLocations_Venues_VenueId",
                table: "VenueLocations",
                column: "VenueId",
                principalTable: "Venues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChapterContactMessageReplies_ChapterContactMessages_ChapterContactMessageId",
                table: "ChapterContactMessageReplies");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterConversationMessages_ChapterConversations_ChapterConversationId",
                table: "ChapterConversationMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterPropertyOptions_ChapterProperties_ChapterPropertyId",
                table: "ChapterPropertyOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_EventComments_EventComments_ParentEventCommentId",
                table: "EventComments");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Venues_VenueId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberProperties_ChapterProperties_ChapterPropertyId",
                table: "MemberProperties");

            migrationBuilder.DropForeignKey(
                name: "FK_VenueLocations_Venues_VenueId",
                table: "VenueLocations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Venues",
                table: "Venues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NewChapterTopics",
                table: "NewChapterTopics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventWaitlistMembers",
                table: "EventWaitlistMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventTicketPayments",
                table: "EventTicketPayments");

            migrationBuilder.DropIndex(
                name: "IX_Events_VenueId",
                table: "Events");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventHosts",
                table: "EventHosts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventEmails",
                table: "EventEmails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventComments",
                table: "EventComments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterQuestions",
                table: "ChapterQuestions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterPropertyOptions",
                table: "ChapterPropertyOptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterProperties",
                table: "ChapterProperties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterPages",
                table: "ChapterPages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterEmails",
                table: "ChapterEmails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterConversations",
                table: "ChapterConversations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterConversationMessages",
                table: "ChapterConversationMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterContactMessages",
                table: "ChapterContactMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterContactMessageReplies",
                table: "ChapterContactMessageReplies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterAdminMembers",
                table: "ChapterAdminMembers");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Venues",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE Venues SET VenueId = Id WHERE VenueId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "VenueId",
                table: "Venues",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "NewChapterTopics",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE NewChapterTopics SET NewChapterTopicId = Id WHERE NewChapterTopicId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "NewChapterTopicId",
                table: "NewChapterTopics",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "EventWaitlistMembers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE EventWaitlistMembers SET EventWaitlistMemberId = Id WHERE EventWaitlistMemberId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "EventWaitlistMemberId",
                table: "EventWaitlistMembers",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "EventTicketPayments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE EventTicketPayments SET EventTicketPaymentId = Id WHERE EventTicketPaymentId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "EventTicketPaymentId",
                table: "EventTicketPayments",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "EventHosts",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE EventHosts SET EventHostId = Id WHERE EventHostId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "EventHostId",
                table: "EventHosts",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "EventEmails",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE EventEmails SET EventEmailId = Id WHERE EventEmailId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "EventEmailId",
                table: "EventEmails",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "EventComments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE EventComments SET EventCommentId = Id WHERE EventCommentId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "EventCommentId",
                table: "EventComments",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ChapterQuestions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE ChapterQuestions SET ChapterQuestionId = Id WHERE ChapterQuestionId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterQuestionId",
                table: "ChapterQuestions",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ChapterPropertyOptions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE ChapterPropertyOptions SET ChapterPropertyOptionId = Id WHERE ChapterPropertyOptionId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterPropertyOptionId",
                table: "ChapterPropertyOptions",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ChapterProperties",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE ChapterProperties SET ChapterPropertyId = Id WHERE ChapterPropertyId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterPropertyId",
                table: "ChapterProperties",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ChapterPages",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE ChapterPages SET ChapterPageId = Id WHERE ChapterPageId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterPageId",
                table: "ChapterPages",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ChapterEmails",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE ChapterEmails SET ChapterEmailId = Id WHERE ChapterEmailId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterEmailId",
                table: "ChapterEmails",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ChapterConversations",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE ChapterConversations SET ChapterConversationId = Id WHERE ChapterConversationId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterConversationId",
                table: "ChapterConversations",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ChapterConversationMessages",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE ChapterConversationMessages SET ChapterConversationMessageId = Id WHERE ChapterConversationMessageId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterConversationMessageId",
                table: "ChapterConversationMessages",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ChapterContactMessages",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE ChapterContactMessages SET ChapterContactMessageId = Id WHERE ChapterContactMessageId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterContactMessageId",
                table: "ChapterContactMessages",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ChapterContactMessageReplies",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE ChapterContactMessageReplies SET ChapterContactMessageReplyId = Id WHERE ChapterContactMessageReplyId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterContactMessageReplyId",
                table: "ChapterContactMessageReplies",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ChapterAdminMembers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE ChapterAdminMembers SET ChapterAdminMemberId = Id WHERE ChapterAdminMemberId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterAdminMemberId",
                table: "ChapterAdminMembers",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Venues",
                table: "Venues",
                column: "VenueId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NewChapterTopics",
                table: "NewChapterTopics",
                column: "NewChapterTopicId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventWaitlistMembers",
                table: "EventWaitlistMembers",
                column: "EventWaitlistMemberId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventTicketPayments",
                table: "EventTicketPayments",
                column: "EventTicketPaymentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventHosts",
                table: "EventHosts",
                column: "EventHostId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventEmails",
                table: "EventEmails",
                column: "EventEmailId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventComments",
                table: "EventComments",
                column: "EventCommentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterQuestions",
                table: "ChapterQuestions",
                column: "ChapterQuestionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterPropertyOptions",
                table: "ChapterPropertyOptions",
                column: "ChapterPropertyOptionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterProperties",
                table: "ChapterProperties",
                column: "ChapterPropertyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterPages",
                table: "ChapterPages",
                column: "ChapterPageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterEmails",
                table: "ChapterEmails",
                column: "ChapterEmailId")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterConversations",
                table: "ChapterConversations",
                column: "ChapterConversationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterConversationMessages",
                table: "ChapterConversationMessages",
                column: "ChapterConversationMessageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterContactMessages",
                table: "ChapterContactMessages",
                column: "ChapterContactMessageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterContactMessageReplies",
                table: "ChapterContactMessageReplies",
                column: "ChapterContactMessageReplyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterAdminMembers",
                table: "ChapterAdminMembers",
                column: "ChapterAdminMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterContactMessageReplies_ChapterContactMessages_ChapterContactMessageId",
                table: "ChapterContactMessageReplies",
                column: "ChapterContactMessageId",
                principalTable: "ChapterContactMessages",
                principalColumn: "ChapterContactMessageId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterConversationMessages_ChapterConversations_ChapterConversationId",
                table: "ChapterConversationMessages",
                column: "ChapterConversationId",
                principalTable: "ChapterConversations",
                principalColumn: "ChapterConversationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterPropertyOptions_ChapterProperties_ChapterPropertyId",
                table: "ChapterPropertyOptions",
                column: "ChapterPropertyId",
                principalTable: "ChapterProperties",
                principalColumn: "ChapterPropertyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventComments_EventComments_ParentEventCommentId",
                table: "EventComments",
                column: "ParentEventCommentId",
                principalTable: "EventComments",
                principalColumn: "EventCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberProperties_ChapterProperties_ChapterPropertyId",
                table: "MemberProperties",
                column: "ChapterPropertyId",
                principalTable: "ChapterProperties",
                principalColumn: "ChapterPropertyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VenueLocations_Venues_VenueId",
                table: "VenueLocations",
                column: "VenueId",
                principalTable: "Venues",
                principalColumn: "VenueId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
