using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Emails;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <summary>
    /// Rewrites the member-import invitation to say what it does: an imported member is invited, not added.
    /// They hold no membership until they accept, and the link goes to the join page where they accept it.
    /// </summary>
    /// <remarks>
    /// The site's wording only. A group that has overridden this template keeps its own row in ChapterEmails,
    /// which is the group's copy to change rather than this migration's.
    /// </remarks>
    public partial class EmailsMemberImportInviteReword : Migration
    {
        private const string NewBody =
            """
            <p>You have been invited to join {group.name}.</p>
            <p>Accept your invitation using the link below.</p>
            <p><a href="{group.urls.join}">{group.urls.join}</a></p>
            """;

        private const string NewSubject = "{title} - You have been invited to join";

        private const string OldBody =
            """
            <p>You have been added to {group.name}.</p>
            <p>Manage your group membership using the link below.</p>
            <p><a href="{group.url}">{group.url}</a></p>
            """;

        private const string OldSubject = "{title} - You have been added to the group";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateEmailWording(
                EmailSchemaEra.IdKey, EmailType.MemberImportInvite, NewSubject, NewBody);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateEmailWording(
                EmailSchemaEra.IdKey, EmailType.MemberImportInvite, OldSubject, OldBody);
        }
    }
}
