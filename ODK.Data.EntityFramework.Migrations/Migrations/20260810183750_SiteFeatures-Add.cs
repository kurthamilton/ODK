using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Features;
using ODK.Data.EntityFramework.Migrations.Enums;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class SiteFeaturesAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* SiteFeatures is not in the EF model, so it was never created by a migration - it
               pre-dates the baseline and exists only in databases restored from production. Every
               statement here is guarded, so this is a no-op against those and does the real work
               against a database built from the migrations alone. It also picks up SiteFeatureType
               values added since, which is why the Theme insert failed in production. */
            migrationBuilder.CreateEnumTable<SiteFeatureType>();
            migrationBuilder.InsertAllEnumValues<SiteFeatureType>();
            migrationBuilder.AddEnumForeignKey<SiteFeatureType>("SiteSubscriptionFeatures", "SiteFeatureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SiteSubscriptionFeatures_SiteFeatures_SiteFeatureId",
                table: "SiteSubscriptionFeatures");

            migrationBuilder.DropEnumTable<SiteFeatureType>();
        }
    }
}
