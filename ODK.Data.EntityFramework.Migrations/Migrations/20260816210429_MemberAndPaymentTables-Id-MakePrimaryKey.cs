using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MemberAndPaymentTablesIdMakePrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* Fill Id for anything the previous migration missed: it backfilled when it was applied, and
               the build that was live for the rest of that deploy knew only the old column. EXEC because
               the generated script batches statements together - see LookupTables-Id-Add. */
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

            migrationBuilder.DropForeignKeys("EventTicketPayments", "PaymentId");

            migrationBuilder.DropForeignKeys("MemberChapterNotificationSettings", "MemberChapterId");

            migrationBuilder.DropForeignKeys("Members", "ReferralId");

            migrationBuilder.DropForeignKeys("MemberSiteSubscriptionLog", "PaymentId");

            migrationBuilder.DropForeignKeys("MemberSiteSubscriptionLog", "SiteSubscriptionPriceId");

            migrationBuilder.DropForeignKeys("MemberSiteSubscriptionLog", "SiteSubscriptionId");

            migrationBuilder.DropForeignKeys("MemberSubscriptionLog", "ChapterSubscriptionId");

            migrationBuilder.DropForeignKeys("MemberSubscriptionLog", "PaymentId");

            migrationBuilder.DropForeignKeys("Referrals", "ReferralCampaignId");

            migrationBuilder.DropForeignKeys("SiteSubscriptionFeatures", "SiteSubscriptionId");

            migrationBuilder.DropForeignKeys("SiteSubscriptionPrices", "SiteSubscriptionId");

            migrationBuilder.DropForeignKeys("SiteSubscriptions", "FallbackSiteSubscriptionId");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SiteSubscriptions",
                table: "SiteSubscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SiteSubscriptionPrices",
                table: "SiteSubscriptionPrices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SiteSubscriptionFeatures",
                table: "SiteSubscriptionFeatures");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Referrals",
                table: "Referrals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReferralCampaigns",
                table: "ReferralCampaigns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payments",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentCheckoutSessions",
                table: "PaymentCheckoutSessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NewMemberTopics",
                table: "NewMemberTopics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MemberProperties",
                table: "MemberProperties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MemberPasswordResetRequests",
                table: "MemberPasswordResetRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MemberNotificationSettings",
                table: "MemberNotificationSettings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MemberChapters",
                table: "MemberChapters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MemberChapterNotificationSettings",
                table: "MemberChapterNotificationSettings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterSubscriptions",
                table: "ChapterSubscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterPaymentAccounts",
                table: "ChapterPaymentAccounts");

            migrationBuilder.AlterColumn<Guid>(
                name: "SiteSubscriptionId",
                table: "SiteSubscriptions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "SiteSubscriptionPriceId",
                table: "SiteSubscriptionPrices",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "SiteSubscriptionFeatureId",
                table: "SiteSubscriptionFeatures",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReferralId",
                table: "Referrals",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReferralCampaignId",
                table: "ReferralCampaigns",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentId",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentCheckoutSessionId",
                table: "PaymentCheckoutSessions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "NewMemberTopicId",
                table: "NewMemberTopics",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "MemberPropertyId",
                table: "MemberProperties",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "MemberPasswordResetRequestId",
                table: "MemberPasswordResetRequests",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "MemberNotificationSettingsId",
                table: "MemberNotificationSettings",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "MemberChapterId",
                table: "MemberChapters",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "MemberChapterNotificationSettingId",
                table: "MemberChapterNotificationSettings",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterSubscriptionId",
                table: "ChapterSubscriptions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterPaymentAccountId",
                table: "ChapterPaymentAccounts",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "SiteSubscriptions",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "SiteSubscriptionPrices",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "SiteSubscriptionFeatures",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Referrals",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ReferralCampaigns",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "PaymentCheckoutSessions",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "NewMemberTopics",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MemberProperties",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MemberPasswordResetRequests",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MemberNotificationSettings",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MemberChapters",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MemberChapterNotificationSettings",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ChapterSubscriptions",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ChapterPaymentAccounts",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SiteSubscriptions",
                table: "SiteSubscriptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SiteSubscriptionPrices",
                table: "SiteSubscriptionPrices",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SiteSubscriptionFeatures",
                table: "SiteSubscriptionFeatures",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Referrals",
                table: "Referrals",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReferralCampaigns",
                table: "ReferralCampaigns",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payments",
                table: "Payments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentCheckoutSessions",
                table: "PaymentCheckoutSessions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NewMemberTopics",
                table: "NewMemberTopics",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MemberProperties",
                table: "MemberProperties",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MemberPasswordResetRequests",
                table: "MemberPasswordResetRequests",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MemberNotificationSettings",
                table: "MemberNotificationSettings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MemberChapters",
                table: "MemberChapters",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MemberChapterNotificationSettings",
                table: "MemberChapterNotificationSettings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterSubscriptions",
                table: "ChapterSubscriptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterPaymentAccounts",
                table: "ChapterPaymentAccounts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EventTicketPayments_Payments_PaymentId",
                table: "EventTicketPayments",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberChapterNotificationSettings_MemberChapters_MemberChapterId",
                table: "MemberChapterNotificationSettings",
                column: "MemberChapterId",
                principalTable: "MemberChapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Referrals_ReferralId",
                table: "Members",
                column: "ReferralId",
                principalTable: "Referrals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSiteSubscriptionLog_Payments_PaymentId",
                table: "MemberSiteSubscriptionLog",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSiteSubscriptionLog_SiteSubscriptionPrices_SiteSubscriptionPriceId",
                table: "MemberSiteSubscriptionLog",
                column: "SiteSubscriptionPriceId",
                principalTable: "SiteSubscriptionPrices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSiteSubscriptionLog_SiteSubscriptions_SiteSubscriptionId",
                table: "MemberSiteSubscriptionLog",
                column: "SiteSubscriptionId",
                principalTable: "SiteSubscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSubscriptionLog_ChapterSubscriptions_ChapterSubscriptionId",
                table: "MemberSubscriptionLog",
                column: "ChapterSubscriptionId",
                principalTable: "ChapterSubscriptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSubscriptionLog_Payments_PaymentId",
                table: "MemberSubscriptionLog",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_ReferralCampaigns_ReferralCampaignId",
                table: "Referrals",
                column: "ReferralCampaignId",
                principalTable: "ReferralCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteSubscriptionFeatures_SiteSubscriptions_SiteSubscriptionId",
                table: "SiteSubscriptionFeatures",
                column: "SiteSubscriptionId",
                principalTable: "SiteSubscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteSubscriptionPrices_SiteSubscriptions_SiteSubscriptionId",
                table: "SiteSubscriptionPrices",
                column: "SiteSubscriptionId",
                principalTable: "SiteSubscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteSubscriptions_SiteSubscriptions_FallbackSiteSubscriptionId",
                table: "SiteSubscriptions",
                column: "FallbackSiteSubscriptionId",
                principalTable: "SiteSubscriptions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventTicketPayments_Payments_PaymentId",
                table: "EventTicketPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberChapterNotificationSettings_MemberChapters_MemberChapterId",
                table: "MemberChapterNotificationSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_Members_Referrals_ReferralId",
                table: "Members");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberSiteSubscriptionLog_Payments_PaymentId",
                table: "MemberSiteSubscriptionLog");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberSiteSubscriptionLog_SiteSubscriptionPrices_SiteSubscriptionPriceId",
                table: "MemberSiteSubscriptionLog");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberSiteSubscriptionLog_SiteSubscriptions_SiteSubscriptionId",
                table: "MemberSiteSubscriptionLog");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberSubscriptionLog_ChapterSubscriptions_ChapterSubscriptionId",
                table: "MemberSubscriptionLog");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberSubscriptionLog_Payments_PaymentId",
                table: "MemberSubscriptionLog");

            migrationBuilder.DropForeignKey(
                name: "FK_Referrals_ReferralCampaigns_ReferralCampaignId",
                table: "Referrals");

            migrationBuilder.DropForeignKey(
                name: "FK_SiteSubscriptionFeatures_SiteSubscriptions_SiteSubscriptionId",
                table: "SiteSubscriptionFeatures");

            migrationBuilder.DropForeignKey(
                name: "FK_SiteSubscriptionPrices_SiteSubscriptions_SiteSubscriptionId",
                table: "SiteSubscriptionPrices");

            migrationBuilder.DropForeignKey(
                name: "FK_SiteSubscriptions_SiteSubscriptions_FallbackSiteSubscriptionId",
                table: "SiteSubscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SiteSubscriptions",
                table: "SiteSubscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SiteSubscriptionPrices",
                table: "SiteSubscriptionPrices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SiteSubscriptionFeatures",
                table: "SiteSubscriptionFeatures");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Referrals",
                table: "Referrals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReferralCampaigns",
                table: "ReferralCampaigns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payments",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentCheckoutSessions",
                table: "PaymentCheckoutSessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NewMemberTopics",
                table: "NewMemberTopics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MemberProperties",
                table: "MemberProperties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MemberPasswordResetRequests",
                table: "MemberPasswordResetRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MemberNotificationSettings",
                table: "MemberNotificationSettings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MemberChapters",
                table: "MemberChapters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MemberChapterNotificationSettings",
                table: "MemberChapterNotificationSettings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterSubscriptions",
                table: "ChapterSubscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterPaymentAccounts",
                table: "ChapterPaymentAccounts");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "SiteSubscriptions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE SiteSubscriptions SET SiteSubscriptionId = Id WHERE SiteSubscriptionId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "SiteSubscriptionId",
                table: "SiteSubscriptions",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "SiteSubscriptionPrices",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE SiteSubscriptionPrices SET SiteSubscriptionPriceId = Id WHERE SiteSubscriptionPriceId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "SiteSubscriptionPriceId",
                table: "SiteSubscriptionPrices",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "SiteSubscriptionFeatures",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE SiteSubscriptionFeatures SET SiteSubscriptionFeatureId = Id WHERE SiteSubscriptionFeatureId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "SiteSubscriptionFeatureId",
                table: "SiteSubscriptionFeatures",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Referrals",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE Referrals SET ReferralId = Id WHERE ReferralId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReferralId",
                table: "Referrals",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ReferralCampaigns",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE ReferralCampaigns SET ReferralCampaignId = Id WHERE ReferralCampaignId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReferralCampaignId",
                table: "ReferralCampaigns",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE Payments SET PaymentId = Id WHERE PaymentId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentId",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "PaymentCheckoutSessions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE PaymentCheckoutSessions SET PaymentCheckoutSessionId = Id WHERE PaymentCheckoutSessionId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentCheckoutSessionId",
                table: "PaymentCheckoutSessions",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "NewMemberTopics",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE NewMemberTopics SET NewMemberTopicId = Id WHERE NewMemberTopicId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "NewMemberTopicId",
                table: "NewMemberTopics",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MemberProperties",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE MemberProperties SET MemberPropertyId = Id WHERE MemberPropertyId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "MemberPropertyId",
                table: "MemberProperties",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MemberPasswordResetRequests",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE MemberPasswordResetRequests SET MemberPasswordResetRequestId = Id WHERE MemberPasswordResetRequestId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "MemberPasswordResetRequestId",
                table: "MemberPasswordResetRequests",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MemberNotificationSettings",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE MemberNotificationSettings SET MemberNotificationSettingsId = Id WHERE MemberNotificationSettingsId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "MemberNotificationSettingsId",
                table: "MemberNotificationSettings",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MemberChapters",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE MemberChapters SET MemberChapterId = Id WHERE MemberChapterId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "MemberChapterId",
                table: "MemberChapters",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MemberChapterNotificationSettings",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE MemberChapterNotificationSettings SET MemberChapterNotificationSettingId = Id WHERE MemberChapterNotificationSettingId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "MemberChapterNotificationSettingId",
                table: "MemberChapterNotificationSettings",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ChapterSubscriptions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE ChapterSubscriptions SET ChapterSubscriptionId = Id WHERE ChapterSubscriptionId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterSubscriptionId",
                table: "ChapterSubscriptions",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ChapterPaymentAccounts",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE ChapterPaymentAccounts SET ChapterPaymentAccountId = Id WHERE ChapterPaymentAccountId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterPaymentAccountId",
                table: "ChapterPaymentAccounts",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SiteSubscriptions",
                table: "SiteSubscriptions",
                column: "SiteSubscriptionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SiteSubscriptionPrices",
                table: "SiteSubscriptionPrices",
                column: "SiteSubscriptionPriceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SiteSubscriptionFeatures",
                table: "SiteSubscriptionFeatures",
                column: "SiteSubscriptionFeatureId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Referrals",
                table: "Referrals",
                column: "ReferralId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReferralCampaigns",
                table: "ReferralCampaigns",
                column: "ReferralCampaignId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payments",
                table: "Payments",
                column: "PaymentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentCheckoutSessions",
                table: "PaymentCheckoutSessions",
                column: "PaymentCheckoutSessionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NewMemberTopics",
                table: "NewMemberTopics",
                column: "NewMemberTopicId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MemberProperties",
                table: "MemberProperties",
                column: "MemberPropertyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MemberPasswordResetRequests",
                table: "MemberPasswordResetRequests",
                column: "MemberPasswordResetRequestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MemberNotificationSettings",
                table: "MemberNotificationSettings",
                column: "MemberNotificationSettingsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MemberChapters",
                table: "MemberChapters",
                column: "MemberChapterId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MemberChapterNotificationSettings",
                table: "MemberChapterNotificationSettings",
                column: "MemberChapterNotificationSettingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterSubscriptions",
                table: "ChapterSubscriptions",
                column: "ChapterSubscriptionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterPaymentAccounts",
                table: "ChapterPaymentAccounts",
                column: "ChapterPaymentAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventTicketPayments_Payments_PaymentId",
                table: "EventTicketPayments",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "PaymentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberChapterNotificationSettings_MemberChapters_MemberChapterId",
                table: "MemberChapterNotificationSettings",
                column: "MemberChapterId",
                principalTable: "MemberChapters",
                principalColumn: "MemberChapterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Referrals_ReferralId",
                table: "Members",
                column: "ReferralId",
                principalTable: "Referrals",
                principalColumn: "ReferralId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSiteSubscriptionLog_Payments_PaymentId",
                table: "MemberSiteSubscriptionLog",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "PaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSiteSubscriptionLog_SiteSubscriptionPrices_SiteSubscriptionPriceId",
                table: "MemberSiteSubscriptionLog",
                column: "SiteSubscriptionPriceId",
                principalTable: "SiteSubscriptionPrices",
                principalColumn: "SiteSubscriptionPriceId");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSiteSubscriptionLog_SiteSubscriptions_SiteSubscriptionId",
                table: "MemberSiteSubscriptionLog",
                column: "SiteSubscriptionId",
                principalTable: "SiteSubscriptions",
                principalColumn: "SiteSubscriptionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSubscriptionLog_ChapterSubscriptions_ChapterSubscriptionId",
                table: "MemberSubscriptionLog",
                column: "ChapterSubscriptionId",
                principalTable: "ChapterSubscriptions",
                principalColumn: "ChapterSubscriptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSubscriptionLog_Payments_PaymentId",
                table: "MemberSubscriptionLog",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "PaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_ReferralCampaigns_ReferralCampaignId",
                table: "Referrals",
                column: "ReferralCampaignId",
                principalTable: "ReferralCampaigns",
                principalColumn: "ReferralCampaignId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteSubscriptionFeatures_SiteSubscriptions_SiteSubscriptionId",
                table: "SiteSubscriptionFeatures",
                column: "SiteSubscriptionId",
                principalTable: "SiteSubscriptions",
                principalColumn: "SiteSubscriptionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteSubscriptionPrices_SiteSubscriptions_SiteSubscriptionId",
                table: "SiteSubscriptionPrices",
                column: "SiteSubscriptionId",
                principalTable: "SiteSubscriptions",
                principalColumn: "SiteSubscriptionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteSubscriptions_SiteSubscriptions_FallbackSiteSubscriptionId",
                table: "SiteSubscriptions",
                column: "FallbackSiteSubscriptionId",
                principalTable: "SiteSubscriptions",
                principalColumn: "SiteSubscriptionId");
        }
    }
}
