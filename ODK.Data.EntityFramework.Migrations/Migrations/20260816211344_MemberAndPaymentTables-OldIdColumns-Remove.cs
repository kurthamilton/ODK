using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <summary>
    /// Drops the columns the member, payment and subscription tables were keyed on before
    /// MemberAndPaymentTables-Id-MakePrimaryKey moved them onto Id. Nothing has read or written them since
    /// the build that migration shipped with.
    /// </summary>
    /// <remarks>
    /// Written by hand: the mapping stopped naming these columns two migrations ago, so the model snapshot
    /// has not known about them since, and EF scaffolds an empty migration. The same goes for the last phase
    /// of every batch.
    /// </remarks>
    public partial class MemberAndPaymentTablesOldIdColumnsRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SiteSubscriptionId",
                table: "SiteSubscriptions");

            migrationBuilder.DropColumn(
                name: "SiteSubscriptionPriceId",
                table: "SiteSubscriptionPrices");

            migrationBuilder.DropColumn(
                name: "SiteSubscriptionFeatureId",
                table: "SiteSubscriptionFeatures");

            migrationBuilder.DropColumn(
                name: "ReferralId",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "ReferralCampaignId",
                table: "ReferralCampaigns");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentCheckoutSessionId",
                table: "PaymentCheckoutSessions");

            migrationBuilder.DropColumn(
                name: "NewMemberTopicId",
                table: "NewMemberTopics");

            migrationBuilder.DropColumn(
                name: "MemberPropertyId",
                table: "MemberProperties");

            migrationBuilder.DropColumn(
                name: "MemberPasswordResetRequestId",
                table: "MemberPasswordResetRequests");

            migrationBuilder.DropColumn(
                name: "MemberNotificationSettingsId",
                table: "MemberNotificationSettings");

            migrationBuilder.DropColumn(
                name: "MemberChapterId",
                table: "MemberChapters");

            migrationBuilder.DropColumn(
                name: "MemberChapterNotificationSettingId",
                table: "MemberChapterNotificationSettings");

            migrationBuilder.DropColumn(
                name: "ChapterSubscriptionId",
                table: "ChapterSubscriptions");

            migrationBuilder.DropColumn(
                name: "ChapterPaymentAccountId",
                table: "ChapterPaymentAccounts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /* Nullable, and only the data restored: these columns were the key when this migration's Up ran,
               but putting that back is the previous migration's Down, not this one's. Going back through both
               returns the tables to where they started, and nothing is lost either way - the values are Id's. */
            migrationBuilder.AddColumn<Guid>(
                name: "SiteSubscriptionId",
                table: "SiteSubscriptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SiteSubscriptionPriceId",
                table: "SiteSubscriptionPrices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SiteSubscriptionFeatureId",
                table: "SiteSubscriptionFeatures",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferralId",
                table: "Referrals",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferralCampaignId",
                table: "ReferralCampaigns",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentId",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentCheckoutSessionId",
                table: "PaymentCheckoutSessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NewMemberTopicId",
                table: "NewMemberTopics",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MemberPropertyId",
                table: "MemberProperties",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MemberPasswordResetRequestId",
                table: "MemberPasswordResetRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MemberNotificationSettingsId",
                table: "MemberNotificationSettings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MemberChapterId",
                table: "MemberChapters",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MemberChapterNotificationSettingId",
                table: "MemberChapterNotificationSettings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ChapterSubscriptionId",
                table: "ChapterSubscriptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ChapterPaymentAccountId",
                table: "ChapterPaymentAccounts",
                type: "uniqueidentifier",
                nullable: true);

            // EXEC because the generated script puts these in the same batch as the columns they write to,
            // and SQL Server binds column names when it parses a batch - see LookupTables-Id-Add.
            migrationBuilder.Sql("EXEC('UPDATE SiteSubscriptions SET SiteSubscriptionId = Id WHERE SiteSubscriptionId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE SiteSubscriptionPrices SET SiteSubscriptionPriceId = Id WHERE SiteSubscriptionPriceId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE SiteSubscriptionFeatures SET SiteSubscriptionFeatureId = Id WHERE SiteSubscriptionFeatureId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Referrals SET ReferralId = Id WHERE ReferralId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ReferralCampaigns SET ReferralCampaignId = Id WHERE ReferralCampaignId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Payments SET PaymentId = Id WHERE PaymentId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE PaymentCheckoutSessions SET PaymentCheckoutSessionId = Id WHERE PaymentCheckoutSessionId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE NewMemberTopics SET NewMemberTopicId = Id WHERE NewMemberTopicId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE MemberProperties SET MemberPropertyId = Id WHERE MemberPropertyId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE MemberPasswordResetRequests SET MemberPasswordResetRequestId = Id WHERE MemberPasswordResetRequestId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE MemberNotificationSettings SET MemberNotificationSettingsId = Id WHERE MemberNotificationSettingsId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE MemberChapters SET MemberChapterId = Id WHERE MemberChapterId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE MemberChapterNotificationSettings SET MemberChapterNotificationSettingId = Id WHERE MemberChapterNotificationSettingId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ChapterSubscriptions SET ChapterSubscriptionId = Id WHERE ChapterSubscriptionId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ChapterPaymentAccounts SET ChapterPaymentAccountId = Id WHERE ChapterPaymentAccountId IS NULL')");
        }
    }
}
