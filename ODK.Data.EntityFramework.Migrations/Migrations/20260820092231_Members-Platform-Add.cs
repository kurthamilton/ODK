using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Platforms;
using ODK.Data.EntityFramework.Migrations.Enums;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MembersPlatformAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlatformTypeId",
                table: "Members",
                type: "int",
                nullable: true);

            migrationBuilder.AddEnumForeignKey<PlatformType>("Members", "PlatformTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropEnumForeignKey<PlatformType>("Members", "PlatformTypeId");

            migrationBuilder.DropColumn(
                name: "PlatformTypeId",
                table: "Members");
        }
    }
}
