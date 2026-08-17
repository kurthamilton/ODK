using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.DataTypes;
using ODK.Core.Events;
using ODK.Core.Issues;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Platforms;
using ODK.Data.EntityFramework.Migrations.Enums;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class EnumTablesIdRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* These tables mirror an enum so other tables can foreign key to its values, and none of them is
               in the EF model, so nothing scaffolds. Each was built by hand before the registry existed and
               keys itself on its own name; the registry expects Id, as the two tables already added through
               it have.

               Renaming a referenced column does not disturb the foreign keys pointing at it - SQL Server
               binds those to a column by id, not by name - so there is no drop and recreate around it. Only
               each lookup table's own key column is renamed. A referencing column such as
               MemberSubscriptionLog.SubscriptionTypeId keeps its name, which states a relationship that
               really is there.

               The create, insert and foreign key statements after each rename are what the registry emits for
               a table it manages. Every one is guarded, so against this schema they do nothing and against a
               database built from the migrations alone they build the table the schema never had. */

            migrationBuilder.RenameEnumIdColumn<ChapterAdminRole>("ChapterAdminRoleId");
            migrationBuilder.CreateEnumTable<ChapterAdminRole>();
            migrationBuilder.InsertAllEnumValues<ChapterAdminRole>();

            migrationBuilder.RenameEnumIdColumn<DataType>("DataTypeId");
            migrationBuilder.CreateEnumTable<DataType>();
            migrationBuilder.InsertAllEnumValues<DataType>();

            migrationBuilder.RenameEnumIdColumn<DistanceUnitType>("DistanceUnitTypeId");
            migrationBuilder.CreateEnumTable<DistanceUnitType>();
            migrationBuilder.InsertAllEnumValues<DistanceUnitType>();
            migrationBuilder.AddEnumForeignKey<DistanceUnitType>("MemberPreferences", "DistanceUnitTypeId");

            /* Listed rather than InsertAllEnumValues: NotInvited is -1, which the invitee form posts to mean
               "remove the invite" and nothing ever stores, so it is no more a valid foreign key target than
               the zero None is. */
            migrationBuilder.RenameEnumIdColumn<EventResponseType>("ResponseTypeId");
            migrationBuilder.CreateEnumTable<EventResponseType>();
            migrationBuilder.InsertEnumValues(
                EventResponseType.Yes,
                EventResponseType.Maybe,
                EventResponseType.No,
                EventResponseType.Waitlist);
            migrationBuilder.AddEnumForeignKey<EventResponseType>("EventResponses", "ResponseTypeId");

            migrationBuilder.RenameEnumIdColumn<IssueStatusType>("IssueStatusTypeId");
            migrationBuilder.CreateEnumTable<IssueStatusType>();
            migrationBuilder.InsertAllEnumValues<IssueStatusType>();
            migrationBuilder.AddEnumForeignKey<IssueStatusType>("Issues", "IssueStatusTypeId");

            migrationBuilder.RenameEnumIdColumn<IssueType>("IssueTypeId");
            migrationBuilder.CreateEnumTable<IssueType>();
            migrationBuilder.InsertAllEnumValues<IssueType>();
            migrationBuilder.AddEnumForeignKey<IssueType>("Issues", "IssueTypeId");

            migrationBuilder.RenameEnumIdColumn<MemberEmailPreferenceType>("MemberEmailPreferenceTypeId");
            migrationBuilder.CreateEnumTable<MemberEmailPreferenceType>();
            migrationBuilder.InsertAllEnumValues<MemberEmailPreferenceType>();
            migrationBuilder.AddEnumForeignKey<MemberEmailPreferenceType>(
                "MemberEmailPreferences", "MemberEmailPreferenceTypeId");

            migrationBuilder.RenameEnumIdColumn<NotificationType>("NotificationTypeId");
            migrationBuilder.CreateEnumTable<NotificationType>();
            migrationBuilder.InsertAllEnumValues<NotificationType>();
            migrationBuilder.AddEnumForeignKey<NotificationType>(
                "MemberChapterNotificationSettings", "NotificationTypeId");
            migrationBuilder.AddEnumForeignKey<NotificationType>("MemberNotificationSettings", "NotificationTypeId");
            migrationBuilder.AddEnumForeignKey<NotificationType>("Notifications", "NotificationTypeId");

            migrationBuilder.RenameEnumIdColumn<PlatformType>("PlatformTypeId");
            migrationBuilder.CreateEnumTable<PlatformType>();
            migrationBuilder.InsertAllEnumValues<PlatformType>();
            migrationBuilder.AddEnumForeignKey<PlatformType>("Chapters", "PlatformTypeId");
            migrationBuilder.AddEnumForeignKey<PlatformType>("SiteEmailSettings", "PlatformTypeId");
            migrationBuilder.AddEnumForeignKey<PlatformType>("SiteSubscriptions", "PlatformTypeId");

            migrationBuilder.RenameEnumIdColumn<SubscriptionType>("SubscriptionTypeId");
            migrationBuilder.CreateEnumTable<SubscriptionType>();
            migrationBuilder.InsertAllEnumValues<SubscriptionType>();
            migrationBuilder.AddEnumForeignKey<SubscriptionType>("ChapterSubscriptions", "SubscriptionTypeId");
            migrationBuilder.AddEnumForeignKey<SubscriptionType>("MemberSubscriptionLog", "SubscriptionTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /* Only the renames reverse. The creates, inserts and foreign keys bring a database up to what the
               registry expects and do nothing to one already there, so reversing them would mean dropping
               tables and rows that other tables reference - destructive where Up was not. */

            migrationBuilder.RenameColumn(name: "Id", table: "ChapterAdminRoles", newName: "ChapterAdminRoleId");
            migrationBuilder.RenameColumn(name: "Id", table: "DataTypes", newName: "DataTypeId");
            migrationBuilder.RenameColumn(name: "Id", table: "DistanceUnitTypes", newName: "DistanceUnitTypeId");
            migrationBuilder.RenameColumn(name: "Id", table: "EventResponseTypes", newName: "ResponseTypeId");
            migrationBuilder.RenameColumn(name: "Id", table: "IssueStatusTypes", newName: "IssueStatusTypeId");
            migrationBuilder.RenameColumn(name: "Id", table: "IssueTypes", newName: "IssueTypeId");
            migrationBuilder.RenameColumn(
                name: "Id", table: "MemberEmailPreferenceTypes", newName: "MemberEmailPreferenceTypeId");
            migrationBuilder.RenameColumn(name: "Id", table: "NotificationTypes", newName: "NotificationTypeId");
            migrationBuilder.RenameColumn(name: "Id", table: "PlatformTypes", newName: "PlatformTypeId");
            migrationBuilder.RenameColumn(name: "Id", table: "SubscriptionTypes", newName: "SubscriptionTypeId");
        }
    }
}
