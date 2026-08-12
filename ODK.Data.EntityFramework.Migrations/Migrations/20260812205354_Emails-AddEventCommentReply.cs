using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Emails;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class EmailsAddEventCommentReply : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // adapted from EmailType.EventComment
            migrationBuilder.InsertEmails(new Email
            {
                HtmlContent = "<p>An event comment has been replied to.</p><p>{comment.text}</p><p><a href=\"{event.url}\">{event.url}</a></p>",
                Overridable = true,
                RecipientType = EmailRecipientType.Members,
                Subject = "{title} - New event comment reply",
                Type = EmailType.EventCommentReply
            });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteEmail(EmailType.EventCommentReply);
        }
    }
}
