using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class SiteSubscriptionsExternalProductIdRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* Hand-written, because the model stopped mentioning this column when
               SiteSubscriptions-SitePaymentProductId-MakeRequired landed, leaving the scaffolder nothing to
               notice. A subscription's product is reached through SitePaymentProductId, and nothing maps or
               reads this column.

               DropColumn, not DropColumnIfExists: InitialCreate created this column, so every database has
               it. DropColumn also clears any default constraint that would block the drop, looked up rather
               than named. No index or foreign key is defined on it. */
            migrationBuilder.DropColumn(
                name: "ExternalProductId",
                table: "SiteSubscriptions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalProductId",
                table: "SiteSubscriptions",
                type: "nvarchar(max)",
                nullable: true);

            /* Restored from the linked product, which is what the column held from the release that began
               writing both. A subscription created before that release carried its own product instead, and
               that value is not recoverable - the platform product is the one a restored release would want
               anyway, since it is where the subscription's prices now live.

               Wrapped in EXEC so it compiles when it runs: a column added to an existing table is not
               resolvable by later statements in the same batch, and a scripted migration is one batch. */
            migrationBuilder.Sql(
                """
                EXEC(N'
                    UPDATE siteSubscription
                    SET siteSubscription.ExternalProductId = sitePaymentProduct.ExternalId
                    FROM SiteSubscriptions siteSubscription
                    INNER JOIN SitePaymentProducts sitePaymentProduct
                        ON sitePaymentProduct.Id = siteSubscription.SitePaymentProductId;');
                """);
        }
    }
}
