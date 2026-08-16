using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class SiteContentTablesIdMakePrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("EXEC('UPDATE SiteQuestions SET Id = SiteQuestionId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE SiteContactMessages SET Id = SiteContactMessageId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE SiteContactMessageReplies SET Id = SiteContactMessageReplyId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Issues SET Id = IssueId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE IssueMessages SET Id = IssueMessageId WHERE Id IS NULL')");

            migrationBuilder.DropForeignKeys("IssueMessages", "IssueId");

            migrationBuilder.DropForeignKeys("SiteContactMessageReplies", "SiteContactMessageId");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SiteQuestions",
                table: "SiteQuestions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SiteContactMessages",
                table: "SiteContactMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SiteContactMessageReplies",
                table: "SiteContactMessageReplies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Issues",
                table: "Issues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IssueMessages",
                table: "IssueMessages");

            migrationBuilder.AlterColumn<Guid>(
                name: "SiteQuestionId",
                table: "SiteQuestions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "SiteContactMessageId",
                table: "SiteContactMessages",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "SiteContactMessageReplyId",
                table: "SiteContactMessageReplies",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "IssueId",
                table: "Issues",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "IssueMessageId",
                table: "IssueMessages",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "SiteQuestions",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "SiteContactMessages",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "SiteContactMessageReplies",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Issues",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "IssueMessages",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SiteQuestions",
                table: "SiteQuestions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SiteContactMessages",
                table: "SiteContactMessages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SiteContactMessageReplies",
                table: "SiteContactMessageReplies",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Issues",
                table: "Issues",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IssueMessages",
                table: "IssueMessages",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IssueMessages_Issues_IssueId",
                table: "IssueMessages",
                column: "IssueId",
                principalTable: "Issues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteContactMessageReplies_SiteContactMessages_SiteContactMessageId",
                table: "SiteContactMessageReplies",
                column: "SiteContactMessageId",
                principalTable: "SiteContactMessages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IssueMessages_Issues_IssueId",
                table: "IssueMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_SiteContactMessageReplies_SiteContactMessages_SiteContactMessageId",
                table: "SiteContactMessageReplies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SiteQuestions",
                table: "SiteQuestions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SiteContactMessages",
                table: "SiteContactMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SiteContactMessageReplies",
                table: "SiteContactMessageReplies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Issues",
                table: "Issues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IssueMessages",
                table: "IssueMessages");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "SiteQuestions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE SiteQuestions SET SiteQuestionId = Id WHERE SiteQuestionId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "SiteQuestionId",
                table: "SiteQuestions",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "SiteContactMessages",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE SiteContactMessages SET SiteContactMessageId = Id WHERE SiteContactMessageId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "SiteContactMessageId",
                table: "SiteContactMessages",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "SiteContactMessageReplies",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE SiteContactMessageReplies SET SiteContactMessageReplyId = Id WHERE SiteContactMessageReplyId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "SiteContactMessageReplyId",
                table: "SiteContactMessageReplies",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Issues",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE Issues SET IssueId = Id WHERE IssueId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "IssueId",
                table: "Issues",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "IssueMessages",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE IssueMessages SET IssueMessageId = Id WHERE IssueMessageId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "IssueMessageId",
                table: "IssueMessages",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SiteQuestions",
                table: "SiteQuestions",
                column: "SiteQuestionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SiteContactMessages",
                table: "SiteContactMessages",
                column: "SiteContactMessageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SiteContactMessageReplies",
                table: "SiteContactMessageReplies",
                column: "SiteContactMessageReplyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Issues",
                table: "Issues",
                column: "IssueId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IssueMessages",
                table: "IssueMessages",
                column: "IssueMessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_IssueMessages_Issues_IssueId",
                table: "IssueMessages",
                column: "IssueId",
                principalTable: "Issues",
                principalColumn: "IssueId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteContactMessageReplies_SiteContactMessages_SiteContactMessageId",
                table: "SiteContactMessageReplies",
                column: "SiteContactMessageId",
                principalTable: "SiteContactMessages",
                principalColumn: "SiteContactMessageId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
