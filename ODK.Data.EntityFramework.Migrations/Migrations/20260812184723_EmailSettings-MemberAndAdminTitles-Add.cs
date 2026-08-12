using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class EmailSettingsMemberAndAdminTitlesAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminTitle",
                table: "SiteEmailSettings",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MemberTitle",
                table: "SiteEmailSettings",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChapterEmailSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdminTitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberTitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChapterEmailSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChapterEmailSettings_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapters",
                        principalColumn: "ChapterId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChapterEmailSettings_ChapterId",
                table: "ChapterEmailSettings",
                column: "ChapterId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChapterEmailSettings");

            migrationBuilder.DropColumn(
                name: "AdminTitle",
                table: "SiteEmailSettings");

            migrationBuilder.DropColumn(
                name: "MemberTitle",
                table: "SiteEmailSettings");
        }
    }
}
