using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class EmailsChapterParametersRename : Migration
    {
        private const string ChapterPrefix = "chapter";

        private const string GroupPrefix = "group";

        /* Every stored string that EmailService interpolates. Templates were written against
           {chapter.*} back when the domain was only chapters; the app now supplies {group.*} and
           nothing else. There is no fallback, so this has to run before the code that drops
           chapter.* - a row missed here renders as literal braces in a sent email. */
        private static readonly (string Table, string[] Columns)[] InterpolatedColumns =
        [
            ("Emails", ["Subject", "Body"]),
            ("ChapterEmails", ["Subject", "HtmlContent"]),
            ("SiteEmailSettings", ["Title", "FromName"]),
            ("ReferralCampaigns", ["EmailSubject", "EmailText"])
        ];

        /* Named in full rather than replacing the "{chapter." prefix, so the statements say exactly
           what they rewrite. This is the complete set EmailService supplies - a token outside it was
           never resolved, and renaming it would only move where it renders as literal braces. */
        private static readonly string[] Parameters = ["baseurl", "fullName", "name"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
            => Rename(migrationBuilder, from: ChapterPrefix, to: GroupPrefix);

        /* Only safe alongside a rollback of the code that supplies group.*, since nothing resolves
           chapter.* on its own. Not an exact inverse either: templates seeded with {group.*} in the
           first place are rewritten to {chapter.*} too. */
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
            => Rename(migrationBuilder, from: GroupPrefix, to: ChapterPrefix);

        private static void Rename(MigrationBuilder migrationBuilder, string from, string to)
        {
            foreach (var (table, columns) in InterpolatedColumns)
            {
                foreach (var parameter in Parameters)
                {
                    var fromToken = Token(from, parameter);
                    var toToken = Token(to, parameter);

                    var set = string.Join(
                        ", ", columns.Select(x => $"[{x}] = REPLACE([{x}], N'{fromToken}', N'{toToken}')"));
                    var where = string.Join(
                        " OR ", columns.Select(x => $"[{x}] LIKE N'%{fromToken}%'"));

                    migrationBuilder.Sql($"UPDATE [{table}] SET {set} WHERE {where};");
                }
            }
        }

        private static string Token(string prefix, string parameter) => $"{{{prefix}.{parameter}}}";
    }
}
