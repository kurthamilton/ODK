using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Emails;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class EmailsInvitesWaitingAdd : Migration
    {
        /* The prompt a group's owner gets when it is published while holding invites an import raised.
           Worded for any number of them: a stored template cannot inflect on the count, so the number
           stands on a line of its own rather than in a sentence that would have to agree with it. */
        private static readonly Email Email = new Email
        {
            BodyHtml =
                """
                <p><strong>{group.name}</strong> is now published, so the invites your import raised can go out.</p>
                <p>Waiting to be sent: {invites.count}</p>
                <p>Nobody is emailed until you send them:</p>
                <p><a href="{invites.url}">{invites.url}</a></p>
                """,
            // The platform's prompt to act on something here, so not the group's to reword.
            IsGroupEmail = false,
            RecipientType = EmailRecipientType.Admins,
            Subject = "{title} - You have invites waiting to be sent",
            Type = EmailType.InvitesWaiting
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
