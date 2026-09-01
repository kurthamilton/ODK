using System;
using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Platforms;
using ODK.Data.EntityFramework.Migrations.Enums;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class SiteEmailSettingsDrop : Migration
    {
        /* Written by hand: the migration before this one took the table out of the snapshot, so there is no
           model change left to scaffold from. The site's email settings are read from configuration now, and
           the build serving when this runs no longer selects the table.

           Down restores the table and its foreign key but not its rows - the values live in configuration,
           which is not something a migration can read. That leaves the same behaviour either way, since
           nothing reads the table, and Up after Down drops an empty table rather than a populated one. */

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Nothing references the table, so its own foreign key to PlatformTypes goes with it.
            migrationBuilder.DropTable(
                name: "SiteEmailSettings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteEmailSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdminTitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FromEmailAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MemberTitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PlatformTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteEmailSettings", x => x.Id);
                });

            migrationBuilder.AddEnumForeignKey<PlatformType>("SiteEmailSettings", "PlatformTypeId");
        }
    }
}
