using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Emails;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class EmailsGroupSubmittedAdminAdd : Migration
    {
        /* Site admins are told twice about a group: once when it is created, which is a heads-up, and once
           when its owner submits it, which is the one that asks for something. The creation email is
           deliberately left saying nothing about approving, so that this one is the only call to act. */
        private static readonly Email Email = new Email
        {
            /* newGroup.name rather than group.name: sent as the site, and the core group.name is what an
               email is addressed from. */
            BodyHtml =
                """
                <p>A group has been submitted for approval</p>
                <p>Name: {newGroup.name}</p>
                <p><a href="{siteadmin.urls.groups}">{siteadmin.urls.groups}</a></p>
                """,
            // The platform's own notification, so not a group's to reword.
            IsGroupEmail = false,
            RecipientType = EmailRecipientType.Admins,
            Subject = "{title} - Group submitted for approval",
            Type = EmailType.GroupSubmittedAdmin
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertEmails(EmailSchemaEra.IdKeyBodyHtml, Email);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteEmail(EmailSchemaEra.IdKeyBodyHtml, Email.Type);
        }
    }
}
