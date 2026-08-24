using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Emails;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class EmailsPaymentNotificationsAdd : Migration
    {
        /* The email a completed payment sends was built in MemberEmailService from an interpolated string. The
           wording below is carried over as it was, with the amount and reference now reaching it as parameters
           so a group can rewrite the sentence around them. */

        private const string MemberBody =
            """
            <p>Your payment of {payment.amount} has been processed.</p>
            <p>Reference: {payment.reference}</p>
            """;

        private const string Subject = "{title} - Payment Received";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertEmails(
                EmailSchemaEra.IdKey,
                new Email
                {
                    HtmlContent = MemberBody,
                    IsGroupEmail = true,
                    RecipientType = EmailRecipientType.Members,
                    Subject = Subject,
                    Type = EmailType.PaymentNotification
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteEmail(EmailSchemaEra.IdKey, EmailType.PaymentNotification);
        }
    }
}
