using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class HubTablesIdMakePrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* Fill Id for anything the previous migration missed: it backfilled when it was applied, and
               the build that was live for the rest of that deploy knew only the old column. EXEC because
               the generated script batches statements together - see LookupTables-Id-Add. */
            migrationBuilder.Sql("EXEC('UPDATE Members SET Id = MemberId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Events SET Id = EventId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Chapters SET Id = ChapterId WHERE Id IS NULL')");

            /* Constraints the scaffolder does not emit a drop for. Chapters.OwnerId and Payments.MemberId
               were added to the model only after the database already enforced them, so EF sees a foreign
               key to create and not the one on the column - and the index it wants is already there too.
               The two on EventTicketPurchases are invisible for a different reason: that table is not in
               the model at all, so EF neither drops them nor puts them back, and they are recreated by
               hand below rather than quietly lost from a table that still holds rows. */
            migrationBuilder.DropForeignKeys("Chapters", "OwnerId");
            migrationBuilder.DropIndexes("Chapters", "OwnerId");
            migrationBuilder.DropForeignKeys("Payments", "MemberId");
            migrationBuilder.DropIndexes("Payments", "MemberId");
            migrationBuilder.DropForeignKeys("MemberActivationTokens", "ChapterId");
            migrationBuilder.DropIndexes("MemberActivationTokens", "ChapterId");
            migrationBuilder.DropForeignKeys("ChapterLinks", "ChapterId");
            migrationBuilder.DropForeignKeys("ChapterEventSettings", "ChapterId");
            migrationBuilder.DropForeignKeys("ChapterMembershipSettings", "ChapterId");
            migrationBuilder.DropForeignKeys("ChapterAdminMembers", "ChapterId");
            migrationBuilder.DropIndexes("ChapterAdminMembers", "ChapterId");
            migrationBuilder.DropForeignKeys("ChapterContactMessages", "ChapterId");
            migrationBuilder.DropIndexes("ChapterContactMessages", "ChapterId");
            migrationBuilder.DropForeignKeys("ChapterQuestions", "ChapterId");
            migrationBuilder.DropIndexes("ChapterQuestions", "ChapterId");
            migrationBuilder.DropForeignKeys("ChapterSubscriptions", "ChapterId");
            migrationBuilder.DropIndexes("ChapterSubscriptions", "ChapterId");
            migrationBuilder.DropForeignKeys("Events", "ChapterId");
            migrationBuilder.DropIndexes("Events", "ChapterId");
            migrationBuilder.DropForeignKeys("Payments", "ChapterId");
            migrationBuilder.DropIndexes("Payments", "ChapterId");
            migrationBuilder.DropForeignKeys("InstagramPosts", "ChapterId");
            migrationBuilder.DropIndexes("InstagramPosts", "ChapterId");
            migrationBuilder.DropForeignKeys("EventTicketPurchases", "EventId");
            migrationBuilder.DropForeignKeys("EventTicketPurchases", "MemberId");
            migrationBuilder.DropForeignKeys("ContactRequests", "ChapterId");
            migrationBuilder.DropForeignKeys("PaymentReconciliations", "ChapterId");
            migrationBuilder.DropForeignKeys("ChapterAdminMembers", "MemberId");

            migrationBuilder.DropForeignKeys("ChapterContactMessageReplies", "MemberId");

            migrationBuilder.DropForeignKeys("ChapterConversationMessages", "MemberId");

            migrationBuilder.DropForeignKeys("ChapterConversations", "ChapterId");

            migrationBuilder.DropForeignKeys("ChapterConversations", "MemberId");

            migrationBuilder.DropForeignKeys("ChapterEmails", "ChapterId");

            migrationBuilder.DropForeignKeys("ChapterEmailSettings", "ChapterId");

            migrationBuilder.DropForeignKeys("ChapterImages", "ChapterId");

            migrationBuilder.DropForeignKeys("ChapterLocations", "ChapterId");

            migrationBuilder.DropForeignKeys("ChapterPages", "ChapterId");

            migrationBuilder.DropForeignKeys("ChapterPaymentAccounts", "ChapterId");

            migrationBuilder.DropForeignKeys("ChapterPaymentAccounts", "OwnerId");

            migrationBuilder.DropForeignKeys("ChapterPaymentSettings", "ChapterId");

            migrationBuilder.DropForeignKeys("ChapterPrivacySettings", "ChapterId");

            migrationBuilder.DropForeignKeys("ChapterProperties", "ChapterId");

            migrationBuilder.DropForeignKeys("ChapterTexts", "ChapterId");

            migrationBuilder.DropForeignKeys("ChapterTopics", "ChapterId");

            migrationBuilder.DropForeignKeys("EventComments", "EventId");

            migrationBuilder.DropForeignKeys("EventComments", "MemberId");

            migrationBuilder.DropForeignKeys("EventEmails", "EventId");

            migrationBuilder.DropForeignKeys("EventHosts", "EventId");

            migrationBuilder.DropForeignKeys("EventHosts", "MemberId");

            migrationBuilder.DropForeignKeys("EventInvites", "EventId");

            migrationBuilder.DropForeignKeys("EventInvites", "MemberId");

            migrationBuilder.DropForeignKeys("EventResponses", "EventId");

            migrationBuilder.DropForeignKeys("EventResponses", "MemberId");

            migrationBuilder.DropForeignKeys("EventTicketPayments", "EventId");

            migrationBuilder.DropForeignKeys("EventTicketSettings", "EventId");

            migrationBuilder.DropForeignKeys("EventTopics", "EventId");

            migrationBuilder.DropForeignKeys("EventWaitlistMembers", "EventId");

            migrationBuilder.DropForeignKeys("EventWaitlistMembers", "MemberId");

            migrationBuilder.DropForeignKeys("FeatureSeenByMembers", "MemberId");

            migrationBuilder.DropForeignKeys("IssueMessages", "MemberId");

            migrationBuilder.DropForeignKeys("Issues", "MemberId");

            migrationBuilder.DropForeignKeys("MemberActivationTokens", "MemberId");

            migrationBuilder.DropForeignKeys("MemberAvatars", "MemberId");

            migrationBuilder.DropForeignKeys("MemberChapters", "ChapterId");

            migrationBuilder.DropForeignKeys("MemberChapters", "MemberId");

            migrationBuilder.DropForeignKeys("MemberEmailAddressUpdateTokens", "MemberId");

            migrationBuilder.DropForeignKeys("MemberEmailPreferences", "MemberId");

            migrationBuilder.DropForeignKeys("MemberLocations", "MemberId");

            migrationBuilder.DropForeignKeys("MemberNotificationSettings", "MemberId");

            migrationBuilder.DropForeignKeys("MemberPasswordResetRequests", "MemberId");

            migrationBuilder.DropForeignKeys("MemberPasswords", "MemberId");

            migrationBuilder.DropForeignKeys("MemberPaymentSettings", "MemberId");

            migrationBuilder.DropForeignKeys("MemberPreferences", "MemberId");

            migrationBuilder.DropForeignKeys("MemberProperties", "MemberId");

            migrationBuilder.DropForeignKeys("MemberSiteSubscriptionLog", "MemberId");

            migrationBuilder.DropForeignKeys("MemberSubscriptionLog", "ChapterId");

            migrationBuilder.DropForeignKeys("MemberSubscriptionLog", "MemberId");

            migrationBuilder.DropForeignKeys("MemberTopics", "MemberId");

            migrationBuilder.DropForeignKeys("NewChapterTopics", "ChapterId");

            migrationBuilder.DropForeignKeys("NewChapterTopics", "MemberId");

            migrationBuilder.DropForeignKeys("NewMemberTopics", "MemberId");

            migrationBuilder.DropForeignKeys("Notifications", "ChapterId");

            migrationBuilder.DropForeignKeys("Notifications", "MemberId");

            migrationBuilder.DropForeignKeys("PaymentCheckoutSessions", "MemberId");

            migrationBuilder.DropForeignKeys("QueuedEmails", "ChapterId");

            migrationBuilder.DropForeignKeys("Referrals", "MemberId");

            migrationBuilder.DropForeignKeys("SiteContactMessageReplies", "MemberId");

            migrationBuilder.DropForeignKeys("Venues", "ChapterId");

            migrationBuilder.DropPrimaryKeyIfExists("Members");

            migrationBuilder.DropPrimaryKeyIfExists("Events");

            migrationBuilder.DropPrimaryKeyIfExists("Chapters");

            migrationBuilder.AlterColumn<Guid>(
                name: "MemberId",
                table: "Members",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "EventId",
                table: "Events",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterId",
                table: "Chapters",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Members",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Events",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Chapters",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Members",
                table: "Members",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Events",
                table: "Events",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chapters",
                table: "Chapters",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ChapterId",
                table: "Payments",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_MemberId",
                table: "Payments",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberActivationTokens_ChapterId",
                table: "MemberActivationTokens",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_InstagramPosts_ChapterId",
                table: "InstagramPosts",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_ChapterId",
                table: "Events",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_ChapterSubscriptions_ChapterId",
                table: "ChapterSubscriptions",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_OwnerId",
                table: "Chapters",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ChapterQuestions_ChapterId",
                table: "ChapterQuestions",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_ChapterContactMessages_ChapterId",
                table: "ChapterContactMessages",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_ChapterAdminMembers_ChapterId",
                table: "ChapterAdminMembers",
                column: "ChapterId");


            migrationBuilder.AddForeignKey(
                name: "FK_ChapterAdminMembers_Members_MemberId",
                table: "ChapterAdminMembers",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);


            migrationBuilder.AddForeignKey(
                name: "FK_ChapterContactMessages_Chapters_ChapterId",
                table: "ChapterContactMessages",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);


            migrationBuilder.AddForeignKey(
                name: "FK_ChapterConversations_Chapters_ChapterId",
                table: "ChapterConversations",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterConversations_Members_MemberId",
                table: "ChapterConversations",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterEmails_Chapters_ChapterId",
                table: "ChapterEmails",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterEmailSettings_Chapters_ChapterId",
                table: "ChapterEmailSettings",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterEventSettings_Chapters_ChapterId",
                table: "ChapterEventSettings",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterImages_Chapters_ChapterId",
                table: "ChapterImages",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterLinks_Chapters_ChapterId",
                table: "ChapterLinks",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterLocations_Chapters_ChapterId",
                table: "ChapterLocations",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterMembershipSettings_Chapters_ChapterId",
                table: "ChapterMembershipSettings",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterPages_Chapters_ChapterId",
                table: "ChapterPages",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterPaymentAccounts_Chapters_ChapterId",
                table: "ChapterPaymentAccounts",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterPaymentAccounts_Members_OwnerId",
                table: "ChapterPaymentAccounts",
                column: "OwnerId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterPaymentSettings_Chapters_ChapterId",
                table: "ChapterPaymentSettings",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterPrivacySettings_Chapters_ChapterId",
                table: "ChapterPrivacySettings",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterProperties_Chapters_ChapterId",
                table: "ChapterProperties",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterQuestions_Chapters_ChapterId",
                table: "ChapterQuestions",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chapters_Members_OwnerId",
                table: "Chapters",
                column: "OwnerId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterSubscriptions_Chapters_ChapterId",
                table: "ChapterSubscriptions",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterTexts_Chapters_ChapterId",
                table: "ChapterTexts",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterTopics_Chapters_ChapterId",
                table: "ChapterTopics",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventComments_Events_EventId",
                table: "EventComments",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventComments_Members_MemberId",
                table: "EventComments",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventEmails_Events_EventId",
                table: "EventEmails",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventHosts_Events_EventId",
                table: "EventHosts",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventHosts_Members_MemberId",
                table: "EventHosts",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventInvites_Events_EventId",
                table: "EventInvites",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventInvites_Members_MemberId",
                table: "EventInvites",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventResponses_Events_EventId",
                table: "EventResponses",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventResponses_Members_MemberId",
                table: "EventResponses",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Chapters_ChapterId",
                table: "Events",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventTicketPayments_Events_EventId",
                table: "EventTicketPayments",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventTicketSettings_Events_EventId",
                table: "EventTicketSettings",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventTopics_Events_EventId",
                table: "EventTopics",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventWaitlistMembers_Events_EventId",
                table: "EventWaitlistMembers",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventWaitlistMembers_Members_MemberId",
                table: "EventWaitlistMembers",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FeatureSeenByMembers_Members_MemberId",
                table: "FeatureSeenByMembers",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InstagramPosts_Chapters_ChapterId",
                table: "InstagramPosts",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IssueMessages_Members_MemberId",
                table: "IssueMessages",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_Members_MemberId",
                table: "Issues",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberActivationTokens_Chapters_ChapterId",
                table: "MemberActivationTokens",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberActivationTokens_Members_MemberId",
                table: "MemberActivationTokens",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberAvatars_Members_MemberId",
                table: "MemberAvatars",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberChapters_Chapters_ChapterId",
                table: "MemberChapters",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberChapters_Members_MemberId",
                table: "MemberChapters",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberEmailAddressUpdateTokens_Members_MemberId",
                table: "MemberEmailAddressUpdateTokens",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberEmailPreferences_Members_MemberId",
                table: "MemberEmailPreferences",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberLocations_Members_MemberId",
                table: "MemberLocations",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberNotificationSettings_Members_MemberId",
                table: "MemberNotificationSettings",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberPasswordResetRequests_Members_MemberId",
                table: "MemberPasswordResetRequests",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberPasswords_Members_MemberId",
                table: "MemberPasswords",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberPaymentSettings_Members_MemberId",
                table: "MemberPaymentSettings",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberPreferences_Members_MemberId",
                table: "MemberPreferences",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberProperties_Members_MemberId",
                table: "MemberProperties",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSiteSubscriptionLog_Members_MemberId",
                table: "MemberSiteSubscriptionLog",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSubscriptionLog_Chapters_ChapterId",
                table: "MemberSubscriptionLog",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSubscriptionLog_Members_MemberId",
                table: "MemberSubscriptionLog",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberTopics_Members_MemberId",
                table: "MemberTopics",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NewChapterTopics_Chapters_ChapterId",
                table: "NewChapterTopics",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NewChapterTopics_Members_MemberId",
                table: "NewChapterTopics",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NewMemberTopics_Members_MemberId",
                table: "NewMemberTopics",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);


            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Members_MemberId",
                table: "Notifications",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentCheckoutSessions_Members_MemberId",
                table: "PaymentCheckoutSessions",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Chapters_ChapterId",
                table: "Payments",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Members_MemberId",
                table: "Payments",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QueuedEmails_Chapters_ChapterId",
                table: "QueuedEmails",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_Members_MemberId",
                table: "Referrals",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteContactMessageReplies_Members_MemberId",
                table: "SiteContactMessageReplies",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Venues_Chapters_ChapterId",
                table: "Venues",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            /* Original names kept rather than given EF's: EF does not manage this table, so a name in
               its convention would suggest otherwise. Delete behaviour is what each had before. */
            migrationBuilder.Sql(
                "ALTER TABLE [EventTicketPurchases] ADD CONSTRAINT [FK_EventTicketPurchases_Events] " +
                "FOREIGN KEY ([EventId]) REFERENCES [Events] ([Id])");

            migrationBuilder.Sql(
                "ALTER TABLE [EventTicketPurchases] ADD CONSTRAINT [FK_EventTicketPurchases_Members] " +
                "FOREIGN KEY ([MemberId]) REFERENCES [Members] ([Id]) ON DELETE CASCADE");

            migrationBuilder.Sql(
                "ALTER TABLE [ContactRequests] ADD CONSTRAINT [FK_ContactRequests_Chapters] " +
                "FOREIGN KEY ([ChapterId]) REFERENCES [Chapters] ([Id])");

            migrationBuilder.Sql(
                "ALTER TABLE [PaymentReconciliations] ADD CONSTRAINT [FK_PaymentReconciliations_Chapters] " +
                "FOREIGN KEY ([ChapterId]) REFERENCES [Chapters] ([Id])");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            // See Up: none of these is EF's to manage.
            migrationBuilder.DropForeignKeys("EventTicketPurchases", "EventId");
            migrationBuilder.DropForeignKeys("EventTicketPurchases", "MemberId");
            migrationBuilder.DropForeignKeys("ContactRequests", "ChapterId");
            migrationBuilder.DropForeignKeys("PaymentReconciliations", "ChapterId");
            migrationBuilder.DropForeignKey(
                name: "FK_ChapterAdminMembers_Members_MemberId",
                table: "ChapterAdminMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterContactMessages_Chapters_ChapterId",
                table: "ChapterContactMessages");


            migrationBuilder.DropForeignKey(
                name: "FK_ChapterConversations_Chapters_ChapterId",
                table: "ChapterConversations");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterConversations_Members_MemberId",
                table: "ChapterConversations");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterEmails_Chapters_ChapterId",
                table: "ChapterEmails");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterEmailSettings_Chapters_ChapterId",
                table: "ChapterEmailSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterEventSettings_Chapters_ChapterId",
                table: "ChapterEventSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterImages_Chapters_ChapterId",
                table: "ChapterImages");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterLinks_Chapters_ChapterId",
                table: "ChapterLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterLocations_Chapters_ChapterId",
                table: "ChapterLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterMembershipSettings_Chapters_ChapterId",
                table: "ChapterMembershipSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterPages_Chapters_ChapterId",
                table: "ChapterPages");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterPaymentAccounts_Chapters_ChapterId",
                table: "ChapterPaymentAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterPaymentAccounts_Members_OwnerId",
                table: "ChapterPaymentAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterPaymentSettings_Chapters_ChapterId",
                table: "ChapterPaymentSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterPrivacySettings_Chapters_ChapterId",
                table: "ChapterPrivacySettings");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterProperties_Chapters_ChapterId",
                table: "ChapterProperties");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterQuestions_Chapters_ChapterId",
                table: "ChapterQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_Chapters_Members_OwnerId",
                table: "Chapters");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterSubscriptions_Chapters_ChapterId",
                table: "ChapterSubscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterTexts_Chapters_ChapterId",
                table: "ChapterTexts");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterTopics_Chapters_ChapterId",
                table: "ChapterTopics");

            migrationBuilder.DropForeignKey(
                name: "FK_EventComments_Events_EventId",
                table: "EventComments");

            migrationBuilder.DropForeignKey(
                name: "FK_EventComments_Members_MemberId",
                table: "EventComments");

            migrationBuilder.DropForeignKey(
                name: "FK_EventEmails_Events_EventId",
                table: "EventEmails");

            migrationBuilder.DropForeignKey(
                name: "FK_EventHosts_Events_EventId",
                table: "EventHosts");

            migrationBuilder.DropForeignKey(
                name: "FK_EventHosts_Members_MemberId",
                table: "EventHosts");

            migrationBuilder.DropForeignKey(
                name: "FK_EventInvites_Events_EventId",
                table: "EventInvites");

            migrationBuilder.DropForeignKey(
                name: "FK_EventInvites_Members_MemberId",
                table: "EventInvites");

            migrationBuilder.DropForeignKey(
                name: "FK_EventResponses_Events_EventId",
                table: "EventResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_EventResponses_Members_MemberId",
                table: "EventResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Chapters_ChapterId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_EventTicketPayments_Events_EventId",
                table: "EventTicketPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_EventTicketSettings_Events_EventId",
                table: "EventTicketSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_EventTopics_Events_EventId",
                table: "EventTopics");

            migrationBuilder.DropForeignKey(
                name: "FK_EventWaitlistMembers_Events_EventId",
                table: "EventWaitlistMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_EventWaitlistMembers_Members_MemberId",
                table: "EventWaitlistMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_FeatureSeenByMembers_Members_MemberId",
                table: "FeatureSeenByMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_InstagramPosts_Chapters_ChapterId",
                table: "InstagramPosts");

            migrationBuilder.DropForeignKey(
                name: "FK_IssueMessages_Members_MemberId",
                table: "IssueMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_Issues_Members_MemberId",
                table: "Issues");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberActivationTokens_Chapters_ChapterId",
                table: "MemberActivationTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberActivationTokens_Members_MemberId",
                table: "MemberActivationTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberAvatars_Members_MemberId",
                table: "MemberAvatars");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberChapters_Chapters_ChapterId",
                table: "MemberChapters");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberChapters_Members_MemberId",
                table: "MemberChapters");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberEmailAddressUpdateTokens_Members_MemberId",
                table: "MemberEmailAddressUpdateTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberEmailPreferences_Members_MemberId",
                table: "MemberEmailPreferences");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberLocations_Members_MemberId",
                table: "MemberLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberNotificationSettings_Members_MemberId",
                table: "MemberNotificationSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberPasswordResetRequests_Members_MemberId",
                table: "MemberPasswordResetRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberPasswords_Members_MemberId",
                table: "MemberPasswords");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberPaymentSettings_Members_MemberId",
                table: "MemberPaymentSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberPreferences_Members_MemberId",
                table: "MemberPreferences");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberProperties_Members_MemberId",
                table: "MemberProperties");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberSiteSubscriptionLog_Members_MemberId",
                table: "MemberSiteSubscriptionLog");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberSubscriptionLog_Chapters_ChapterId",
                table: "MemberSubscriptionLog");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberSubscriptionLog_Members_MemberId",
                table: "MemberSubscriptionLog");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberTopics_Members_MemberId",
                table: "MemberTopics");

            migrationBuilder.DropForeignKey(
                name: "FK_NewChapterTopics_Chapters_ChapterId",
                table: "NewChapterTopics");

            migrationBuilder.DropForeignKey(
                name: "FK_NewChapterTopics_Members_MemberId",
                table: "NewChapterTopics");

            migrationBuilder.DropForeignKey(
                name: "FK_NewMemberTopics_Members_MemberId",
                table: "NewMemberTopics");


            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Members_MemberId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentCheckoutSessions_Members_MemberId",
                table: "PaymentCheckoutSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Chapters_ChapterId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Members_MemberId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_QueuedEmails_Chapters_ChapterId",
                table: "QueuedEmails");

            migrationBuilder.DropForeignKey(
                name: "FK_Referrals_Members_MemberId",
                table: "Referrals");

            migrationBuilder.DropForeignKey(
                name: "FK_SiteContactMessageReplies_Members_MemberId",
                table: "SiteContactMessageReplies");

            migrationBuilder.DropForeignKey(
                name: "FK_Venues_Chapters_ChapterId",
                table: "Venues");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ChapterId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_MemberId",
                table: "Payments");

            migrationBuilder.DropPrimaryKeyIfExists("Members");

            migrationBuilder.DropIndex(
                name: "IX_MemberActivationTokens_ChapterId",
                table: "MemberActivationTokens");

            migrationBuilder.DropIndex(
                name: "IX_InstagramPosts_ChapterId",
                table: "InstagramPosts");

            migrationBuilder.DropPrimaryKeyIfExists("Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_ChapterId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_ChapterSubscriptions_ChapterId",
                table: "ChapterSubscriptions");

            migrationBuilder.DropPrimaryKeyIfExists("Chapters");

            migrationBuilder.DropIndex(
                name: "IX_Chapters_OwnerId",
                table: "Chapters");

            migrationBuilder.DropIndex(
                name: "IX_ChapterQuestions_ChapterId",
                table: "ChapterQuestions");

            migrationBuilder.DropIndex(
                name: "IX_ChapterContactMessages_ChapterId",
                table: "ChapterContactMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChapterAdminMembers_ChapterId",
                table: "ChapterAdminMembers");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Members",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE Members SET MemberId = Id WHERE MemberId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "MemberId",
                table: "Members",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Events",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE Events SET EventId = Id WHERE EventId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "EventId",
                table: "Events",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Chapters",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE Chapters SET ChapterId = Id WHERE ChapterId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterId",
                table: "Chapters",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Members",
                table: "Members",
                column: "MemberId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Events",
                table: "Events",
                column: "EventId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chapters",
                table: "Chapters",
                column: "ChapterId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterAdminMembers_Members_MemberId",
                table: "ChapterAdminMembers",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterContactMessageReplies_Members_MemberId",
                table: "ChapterContactMessageReplies",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterConversationMessages_Members_MemberId",
                table: "ChapterConversationMessages",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterConversations_Chapters_ChapterId",
                table: "ChapterConversations",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "ChapterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterConversations_Members_MemberId",
                table: "ChapterConversations",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterEmails_Chapters_ChapterId",
                table: "ChapterEmails",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "ChapterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterEmailSettings_Chapters_ChapterId",
                table: "ChapterEmailSettings",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "ChapterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterImages_Chapters_ChapterId",
                table: "ChapterImages",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "ChapterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterLocations_Chapters_ChapterId",
                table: "ChapterLocations",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "ChapterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterPages_Chapters_ChapterId",
                table: "ChapterPages",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "ChapterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterPaymentAccounts_Chapters_ChapterId",
                table: "ChapterPaymentAccounts",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "ChapterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterPaymentAccounts_Members_OwnerId",
                table: "ChapterPaymentAccounts",
                column: "OwnerId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterPaymentSettings_Chapters_ChapterId",
                table: "ChapterPaymentSettings",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "ChapterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterPrivacySettings_Chapters_ChapterId",
                table: "ChapterPrivacySettings",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "ChapterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterProperties_Chapters_ChapterId",
                table: "ChapterProperties",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "ChapterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterTexts_Chapters_ChapterId",
                table: "ChapterTexts",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "ChapterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterTopics_Chapters_ChapterId",
                table: "ChapterTopics",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "ChapterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventComments_Events_EventId",
                table: "EventComments",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventComments_Members_MemberId",
                table: "EventComments",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventEmails_Events_EventId",
                table: "EventEmails",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventHosts_Events_EventId",
                table: "EventHosts",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventHosts_Members_MemberId",
                table: "EventHosts",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventInvites_Events_EventId",
                table: "EventInvites",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventInvites_Members_MemberId",
                table: "EventInvites",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventResponses_Events_EventId",
                table: "EventResponses",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventResponses_Members_MemberId",
                table: "EventResponses",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventTicketPayments_Events_EventId",
                table: "EventTicketPayments",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventTicketSettings_Events_EventId",
                table: "EventTicketSettings",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventTopics_Events_EventId",
                table: "EventTopics",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventWaitlistMembers_Events_EventId",
                table: "EventWaitlistMembers",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventWaitlistMembers_Members_MemberId",
                table: "EventWaitlistMembers",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FeatureSeenByMembers_Members_MemberId",
                table: "FeatureSeenByMembers",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IssueMessages_Members_MemberId",
                table: "IssueMessages",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_Members_MemberId",
                table: "Issues",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberActivationTokens_Members_MemberId",
                table: "MemberActivationTokens",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberAvatars_Members_MemberId",
                table: "MemberAvatars",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberChapters_Chapters_ChapterId",
                table: "MemberChapters",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "ChapterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberChapters_Members_MemberId",
                table: "MemberChapters",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberEmailAddressUpdateTokens_Members_MemberId",
                table: "MemberEmailAddressUpdateTokens",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberEmailPreferences_Members_MemberId",
                table: "MemberEmailPreferences",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberLocations_Members_MemberId",
                table: "MemberLocations",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberNotificationSettings_Members_MemberId",
                table: "MemberNotificationSettings",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberPasswordResetRequests_Members_MemberId",
                table: "MemberPasswordResetRequests",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberPasswords_Members_MemberId",
                table: "MemberPasswords",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberPaymentSettings_Members_MemberId",
                table: "MemberPaymentSettings",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberPreferences_Members_MemberId",
                table: "MemberPreferences",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberProperties_Members_MemberId",
                table: "MemberProperties",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSiteSubscriptionLog_Members_MemberId",
                table: "MemberSiteSubscriptionLog",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSubscriptionLog_Chapters_ChapterId",
                table: "MemberSubscriptionLog",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "ChapterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSubscriptionLog_Members_MemberId",
                table: "MemberSubscriptionLog",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberTopics_Members_MemberId",
                table: "MemberTopics",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NewChapterTopics_Chapters_ChapterId",
                table: "NewChapterTopics",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "ChapterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NewChapterTopics_Members_MemberId",
                table: "NewChapterTopics",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NewMemberTopics_Members_MemberId",
                table: "NewMemberTopics",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Chapters_ChapterId",
                table: "Notifications",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "ChapterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Members_MemberId",
                table: "Notifications",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentCheckoutSessions_Members_MemberId",
                table: "PaymentCheckoutSessions",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QueuedEmails_Chapters_ChapterId",
                table: "QueuedEmails",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "ChapterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_Members_MemberId",
                table: "Referrals",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteContactMessageReplies_Members_MemberId",
                table: "SiteContactMessageReplies",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Venues_Chapters_ChapterId",
                table: "Venues",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "ChapterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql(
                "ALTER TABLE [EventTicketPurchases] ADD CONSTRAINT [FK_EventTicketPurchases_Events] " +
                "FOREIGN KEY ([EventId]) REFERENCES [Events] ([EventId])");

            migrationBuilder.Sql(
                "ALTER TABLE [EventTicketPurchases] ADD CONSTRAINT [FK_EventTicketPurchases_Members] " +
                "FOREIGN KEY ([MemberId]) REFERENCES [Members] ([MemberId]) ON DELETE CASCADE");

            migrationBuilder.Sql(
                "ALTER TABLE [ContactRequests] ADD CONSTRAINT [FK_ContactRequests_Chapters] " +
                "FOREIGN KEY ([ChapterId]) REFERENCES [Chapters] ([ChapterId])");

            migrationBuilder.Sql(
                "ALTER TABLE [PaymentReconciliations] ADD CONSTRAINT [FK_PaymentReconciliations_Chapters] " +
                "FOREIGN KEY ([ChapterId]) REFERENCES [Chapters] ([ChapterId])");
        }
    }
}
