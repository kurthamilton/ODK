using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ChapterPropertiesRebuildForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*drop existing*/
            migrationBuilder.DropForeignKey(
                name: "FK_ChapterProperties_Chapters",
                table: "ChapterProperties");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterPropertyOptions",
                table: "ChapterPropertyOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberProperties_ChapterProperties",
                table: "MemberProperties");

            /*auto-generated*/
            migrationBuilder.CreateIndex(
                name: "IX_MemberProperties_ChapterPropertyId",
                table: "MemberProperties",
                column: "ChapterPropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_ChapterPropertyOptions_ChapterPropertyId",
                table: "ChapterPropertyOptions",
                column: "ChapterPropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_ChapterProperties_ChapterId",
                table: "ChapterProperties",
                column: "ChapterId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterProperties_Chapters_ChapterId",
                table: "ChapterProperties",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "ChapterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterPropertyOptions_ChapterProperties_ChapterPropertyId",
                table: "ChapterPropertyOptions",
                column: "ChapterPropertyId",
                principalTable: "ChapterProperties",
                principalColumn: "ChapterPropertyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberProperties_ChapterProperties_ChapterPropertyId",
                table: "MemberProperties",
                column: "ChapterPropertyId",
                principalTable: "ChapterProperties",
                principalColumn: "ChapterPropertyId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChapterProperties_Chapters_ChapterId",
                table: "ChapterProperties");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterPropertyOptions_ChapterProperties_ChapterPropertyId",
                table: "ChapterPropertyOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberProperties_ChapterProperties_ChapterPropertyId",
                table: "MemberProperties");

            migrationBuilder.DropIndex(
                name: "IX_MemberProperties_ChapterPropertyId",
                table: "MemberProperties");

            migrationBuilder.DropIndex(
                name: "IX_ChapterPropertyOptions_ChapterPropertyId",
                table: "ChapterPropertyOptions");

            migrationBuilder.DropIndex(
                name: "IX_ChapterProperties_ChapterId",
                table: "ChapterProperties");
        }
    }
}