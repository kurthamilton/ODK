using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Platforms;
using ODK.Data.EntityFramework.Migrations.Enums;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class SiteContactMessagesPlatformAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* Nullable to begin with, and tightened below once every row has a platform. No defaultValue,
               which the scaffolder supplies as 0: that is None, which InsertAllEnumValues deliberately
               leaves out of the lookup table, so every row would fail the foreign key added at the end. */
            migrationBuilder.AddColumn<int>(
                name: "PlatformTypeId",
                table: "SiteContactMessages",
                type: "int",
                nullable: true);

            // Every message sent before the column existed becomes the default platform's.
            migrationBuilder.Sql(
                $"""
                UPDATE [SiteContactMessages]
                SET [PlatformTypeId] = {(int)PlatformType.GroupSquirrel}
                WHERE [PlatformTypeId] IS NULL;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "PlatformTypeId",
                table: "SiteContactMessages",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddEnumForeignKey<PlatformType>("SiteContactMessages", "PlatformTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /* Dropping the column loses which platform each message came through, and a second Up puts every
               message back on the default platform. Only messages received while the column existed can be
               wrong that way, and a site admin moves one by hand; there is nowhere else the value survives,
               short of a backup. */
            migrationBuilder.DropEnumForeignKey<PlatformType>("SiteContactMessages", "PlatformTypeId");

            migrationBuilder.DropColumn(
                name: "PlatformTypeId",
                table: "SiteContactMessages");
        }
    }
}
