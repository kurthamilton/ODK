using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class SiteSubscriptionsFreeAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Free",
                table: "SiteSubscriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Before the flag, a free subscription was expressed as a zero-amount price, so those become
            // flagged ones. The EXISTS is what separates them from a subscription with no prices at all:
            // that one is not free but unusable, and flagging it would silently make it available.
            migrationBuilder.Sql(@"
UPDATE SiteSubscriptions
SET Free = 1
WHERE EXISTS (
        SELECT 1 FROM SiteSubscriptionPrices price
        WHERE price.SiteSubscriptionId = SiteSubscriptions.Id)
  AND NOT EXISTS (
        SELECT 1 FROM SiteSubscriptionPrices price
        WHERE price.SiteSubscriptionId = SiteSubscriptions.Id AND price.Amount > 0);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /* Dropping the column loses the flag for a free subscription that has no zero-amount price to
               re-derive it from - one created after this migration. Up then restores the flag for every
               subscription that still carries such a price, which is the correct forward state for the rows
               it can see; a priceless free one has to be re-flagged by hand. */
            migrationBuilder.DropColumn(
                name: "Free",
                table: "SiteSubscriptions");
        }
    }
}
