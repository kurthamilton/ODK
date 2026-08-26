using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Platforms;
using ODK.Data.EntityFramework.Migrations.Enums;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class SitePaymentSettingsPlatformAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* Nullable to begin with, and tightened below once every row has a platform. No defaultValue,
               which the scaffolder supplies as 0: that is None, which InsertAllEnumValues deliberately
               leaves out of the lookup table, so every row would fail the foreign key added at the end. */
            migrationBuilder.AddColumn<int>(
                name: "PlatformTypeId",
                table: "SitePaymentSettings",
                type: "int",
                nullable: true);

            /* The existing rows keep their ids and become Drunken Knitwits': every payment, connected
               account, subscription and product already recorded against them was made under those API
               keys, so the platform that keeps the row is the one whose Stripe history stays resolvable. */
            migrationBuilder.Sql(
                $"""
                UPDATE SitePaymentSettings
                SET PlatformTypeId = {(int)PlatformType.DrunkenKnitwits}
                WHERE PlatformTypeId IS NULL;
                """);

            /* A copy per remaining platform, carrying the same keys, so each platform resolves its own
               active row and behaves identically until its own account's keys are entered against it.
               Nothing references the copies, which is what makes entering different keys safe. */
            migrationBuilder.Sql(
                $"""
                INSERT INTO SitePaymentSettings
                    (Id, Provider, ApiPublicKey, ApiSecretKey, Active, Name, Commission, Enabled, PlatformTypeId)
                SELECT
                    NEWID(), Provider, ApiPublicKey, ApiSecretKey, Active, Name, Commission, Enabled,
                    {(int)PlatformType.Default}
                FROM SitePaymentSettings
                WHERE PlatformTypeId = {(int)PlatformType.DrunkenKnitwits};
                """);

            migrationBuilder.AlterColumn<int>(
                name: "PlatformTypeId",
                table: "SitePaymentSettings",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddEnumForeignKey<PlatformType>("SitePaymentSettings", "PlatformTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /* The copies go, and have to: without a platform to tell them apart they are duplicates of the
               originals - same keys, same name, both active - and the release this reverts to reads the
               active row as a single row. Every row on the copied platform is deleted, because before this
               migration no row had a platform at all, so each one exists only by way of it. A row something
               already references fails the delete rather than being silently detached. */
            migrationBuilder.DropEnumForeignKey<PlatformType>("SitePaymentSettings", "PlatformTypeId");

            migrationBuilder.Sql(
                $"DELETE FROM SitePaymentSettings WHERE PlatformTypeId = {(int)PlatformType.Default};");

            migrationBuilder.DropColumn(
                name: "PlatformTypeId",
                table: "SitePaymentSettings");
        }
    }
}
