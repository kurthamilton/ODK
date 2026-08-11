using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ChapterEmailsAddEmailsFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .DropConstraintIfExists("ChapterEmails", "PK_ChapterEmails")
                .DropConstraintIfExists("ChapterEmails", "UQ_ChapterEmails_ChapterId_EmailTypeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterEmails",
                table: "ChapterEmails",
                column: "ChapterEmailId")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_ChapterEmails_ChapterId",
                table: "ChapterEmails",
                column: "ChapterId")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_ChapterEmails_ChapterId_EmailTypeId",
                table: "ChapterEmails",
                columns: new[] { "ChapterId", "EmailTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChapterEmails_EmailTypeId",
                table: "ChapterEmails",
                column: "EmailTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterEmails_Chapters_ChapterId",
                table: "ChapterEmails",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "ChapterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterEmails_Emails_EmailTypeId",
                table: "ChapterEmails",
                column: "EmailTypeId",
                principalTable: "Emails",
                principalColumn: "EmailTypeId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChapterEmails_Chapters_ChapterId",
                table: "ChapterEmails");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterEmails_Emails_EmailTypeId",
                table: "ChapterEmails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterEmails",
                table: "ChapterEmails");

            migrationBuilder.DropIndex(
                name: "IX_ChapterEmails_ChapterId",
                table: "ChapterEmails");

            migrationBuilder.DropIndex(
                name: "IX_ChapterEmails_ChapterId_EmailTypeId",
                table: "ChapterEmails");

            migrationBuilder.DropIndex(
                name: "IX_ChapterEmails_EmailTypeId",
                table: "ChapterEmails");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterEmails",
                table: "ChapterEmails",
                column: "ChapterEmailId");
        }
    }
}
