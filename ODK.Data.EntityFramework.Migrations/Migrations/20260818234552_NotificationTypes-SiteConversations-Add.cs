using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Notifications;
using ODK.Data.EntityFramework.Migrations.Enums;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class NotificationTypesSiteConversationsAdd : Migration
    {
        /* NotificationTypes is not in the EF model, so nothing adds these rows on its own: without them,
           every write of Notifications.NotificationTypeId = 7 or 8 fails the foreign key at runtime. */
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
            => migrationBuilder.InsertEnumValues(
                NotificationType.SiteConversationMemberMessage,
                NotificationType.SiteConversationReplies);

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
            => migrationBuilder.DeleteEnumValues(
                NotificationType.SiteConversationMemberMessage,
                NotificationType.SiteConversationReplies);
    }
}
