using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Notifications;
using ODK.Data.EntityFramework.Migrations.Enums;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class NotificationTypesSubscriptionRenewedAdd : Migration
    {
        /* NotificationTypes is not in the EF model, so nothing adds this row on its own: without it,
           every write of Notifications.NotificationTypeId = 9 fails the foreign key at runtime. */
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
            => migrationBuilder.InsertEnumValues(NotificationType.SubscriptionRenewed);

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
            => migrationBuilder.DeleteEnumValues(NotificationType.SubscriptionRenewed);
    }
}
