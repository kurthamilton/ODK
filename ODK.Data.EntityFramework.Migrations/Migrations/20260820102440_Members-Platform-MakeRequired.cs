using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Platforms;
using ODK.Data.EntityFramework.Migrations.Enums;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MembersPlatformMakeRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* Every account raised before the column existed is treated as a Drunken Knitwits one: which
               platform an old account signed up on is not recorded anywhere, so there is nothing to derive
               it from, and the handful that belong to another platform are corrected by hand afterwards. */
            migrationBuilder.Sql(
                "UPDATE [Members] " +
                $"SET [PlatformTypeId] = {(int)PlatformType.DrunkenKnitwits} " +
                "WHERE [PlatformTypeId] IS NULL;");

            // SQL Server will not modify a column a foreign key constraint is defined on, so the key comes
            // off for the alter and goes back on after it.
            migrationBuilder.DropEnumForeignKey<PlatformType>("Members", "PlatformTypeId");

            /* No defaultValue, which the scaffolder supplies as 0: the backfill above leaves no nulls for
               one to fill, and 0 is None, which is deliberately never a valid enum foreign key target. A
               row the backfill missed must fail the alter rather than be given a value nothing means. */
            migrationBuilder.AlterColumn<int>(
                name: "PlatformTypeId",
                table: "Members",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddEnumForeignKey<PlatformType>("Members", "PlatformTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /* The backfilled values stay. Which rows the backfill wrote is not recorded, so none of them can
               be returned to null - and the round trip still lands correctly, because a second Up finds
               nothing to fill and tightens the same column over the same data. */
            migrationBuilder.DropEnumForeignKey<PlatformType>("Members", "PlatformTypeId");

            migrationBuilder.AlterColumn<int>(
                name: "PlatformTypeId",
                table: "Members",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddEnumForeignKey<PlatformType>("Members", "PlatformTypeId");
        }
    }
}
