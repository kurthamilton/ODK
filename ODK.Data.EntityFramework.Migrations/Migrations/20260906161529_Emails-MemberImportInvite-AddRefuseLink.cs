using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Emails;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <summary>
    /// Adds the decline link to the member-import invitation. An import raises an account for an address that
    /// never asked for one, so the invitation has to carry a way out alongside the way in.
    /// </summary>
    /// <remarks>
    /// The site's wording only. A group that has overridden this template keeps its own row in ChapterEmails,
    /// which is the group's copy to change rather than this migration's.
    /// </remarks>
    public partial class EmailsMemberImportInviteAddRefuseLink : Migration
    {
        private const string NewBody =
            """
            <p>You have been invited to join {group.name}.</p>
            <p>Accept your invitation using the link below.</p>
            <p><a href="{group.urls.join}">{group.urls.join}</a></p>
            <p>
                If you would rather not join, you can
                <a href="{group.urls.refuseInvite}">decline the invitation</a>, which deletes the name and
                email address {group.name} holds for you.
            </p>
            """;

        private const string OldBody =
            """
            <p>You have been invited to join {group.name}.</p>
            <p>Accept your invitation using the link below.</p>
            <p><a href="{group.urls.join}">{group.urls.join}</a></p>
            """;

        private const string Subject = "{title} - You have been invited to join";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateEmailWording(
                EmailSchemaEra.IdKeyBodyHtml, EmailType.MemberImportInvite, Subject, NewBody);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateEmailWording(
                EmailSchemaEra.IdKeyBodyHtml, EmailType.MemberImportInvite, Subject, OldBody);
        }
    }
}
