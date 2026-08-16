using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MemberAndPaymentTablesIdAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "SiteSubscriptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "SiteSubscriptionPrices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "SiteSubscriptionFeatures",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Referrals",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ReferralCampaigns",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "PaymentCheckoutSessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "NewMemberTopics",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "MemberProperties",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "MemberPasswordResetRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "MemberNotificationSettings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "MemberChapters",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "MemberChapterNotificationSettings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ChapterSubscriptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ChapterPaymentAccounts",
                type: "uniqueidentifier",
                nullable: true);

            /* Copy the key across. Nullable for now and left that way deliberately: the build running while
               this is applied knows only the old column, so a row it inserts arrives with Id unset. The
               migration that makes Id the key backfills again before it adds the constraint.

               EXEC because the generated script puts the ALTER above and the UPDATE in one batch, and SQL
               Server binds column names when it parses a batch - see LookupTables-Id-Add. */
            migrationBuilder.Sql("EXEC('UPDATE SiteSubscriptions SET Id = SiteSubscriptionId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE SiteSubscriptionPrices SET Id = SiteSubscriptionPriceId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE SiteSubscriptionFeatures SET Id = SiteSubscriptionFeatureId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Referrals SET Id = ReferralId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ReferralCampaigns SET Id = ReferralCampaignId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Payments SET Id = PaymentId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE PaymentCheckoutSessions SET Id = PaymentCheckoutSessionId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE NewMemberTopics SET Id = NewMemberTopicId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE MemberProperties SET Id = MemberPropertyId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE MemberPasswordResetRequests SET Id = MemberPasswordResetRequestId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE MemberNotificationSettings SET Id = MemberNotificationSettingsId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE MemberChapters SET Id = MemberChapterId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE MemberChapterNotificationSettings SET Id = MemberChapterNotificationSettingId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ChapterSubscriptions SET Id = ChapterSubscriptionId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE ChapterPaymentAccounts SET Id = ChapterPaymentAccountId WHERE Id IS NULL')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "SiteSubscriptions");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "SiteSubscriptionPrices");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "SiteSubscriptionFeatures");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ReferralCampaigns");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PaymentCheckoutSessions");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "NewMemberTopics");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "MemberProperties");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "MemberPasswordResetRequests");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "MemberNotificationSettings");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "MemberChapters");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "MemberChapterNotificationSettings");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ChapterSubscriptions");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ChapterPaymentAccounts");
        }
    }
}
