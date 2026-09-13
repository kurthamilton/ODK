using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ChapterMigrationsAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChapterMigrations",
                columns: table => new
                {
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MovedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PreviousPlatformName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChapterMigrations", x => x.ChapterId);
                    table.ForeignKey(
                        name: "FK_ChapterMigrations_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChapterMigrations");
        }
    }
}
