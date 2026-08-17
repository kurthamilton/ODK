using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Emails;
using ODK.Core.Features;
using ODK.Data.EntityFramework.Migrations.Enums;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class EmailsMemberImportAdd : Migration
    {
        /* The two emails an import sends were hard-coded in MemberEmailService. The wording below is
           carried over unchanged apart from a "You application" typo, so imported members see what
           they saw before - but the rows are Overridable, so a group admin can now customise them.

           These templates were written with the {group.*} parameters from the start. At the time
           {chapter.*} also resolved; Emails-ChapterParameters-Rename later dropped that. */

        private const string ActivationBody =
            """
            <p>Your application to join {group.fullName} has been approved.</p>
            <p>Activate your account using the link below.</p>
            <p><a href="{url}">{url}</a></p>
            """;

        private const string InviteBody =
            """
            <p>You have been added to {group.name}.</p>
            <p>Manage your group membership using the link below.</p>
            <p><a href="{url}">{url}</a></p>
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertEmails(
                EmailSchemaEra.TypeIdKey,
                new Email
                {
                    HtmlContent = ActivationBody,
                    Overridable = true,
                    Subject = "{title} - Activate your account",
                    Type = EmailType.MemberImportActivation
                },
                new Email
                {
                    HtmlContent = InviteBody,
                    Overridable = true,
                    Subject = "{title} - You have been added to the group",
                    Type = EmailType.MemberImportInvite
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Emails",
                keyColumn: "EmailTypeId",
                keyValues:
                [
                    (int)EmailType.MemberImportActivation,
                    (int)EmailType.MemberImportInvite
                ]);
        }
    }
}
