using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Features;
using ODK.Data.EntityFramework.Migrations.Enums;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class SiteFeaturesCustomEmailsAdd : Migration
    {
        /* SiteFeatures is not in the EF model, so nothing adds the row on its own: without this, every
           write of SiteSubscriptionFeatures.SiteFeatureId = 10 fails the foreign key at runtime. */
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
            => migrationBuilder.InsertEnumValues(SiteFeatureType.CustomEmails);

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
            => migrationBuilder.DeleteEnumValues(SiteFeatureType.CustomEmails);
    }
}
