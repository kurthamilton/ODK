using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class EmailsGroupFullNameRename : Migration
    {
        private const string CamelCaseToken = "{group.fullName}";

        private const string LowerCaseToken = "{group.fullname}";

        /* Same set as Emails-ChapterParameters-Rename. Deliberately repeated rather than shared with
           it: a migration is a record of what ran on a given day, and a list two of them read from
           would silently rewrite that history the next time a table is added to it. */
        private static readonly (string Table, string[] Columns)[] InterpolatedColumns =
        [
            ("Emails", ["Subject", "Body"]),
            ("ChapterEmails", ["Subject", "HtmlContent"]),
            ("SiteEmailSettings", ["Title", "FromName"]),
            ("ReferralCampaigns", ["EmailSubject", "EmailText"])
        ];
        
        protected override void Up(MigrationBuilder migrationBuilder)
            => Rename(migrationBuilder, from: CamelCaseToken, to: LowerCaseToken);

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
            => Rename(migrationBuilder, from: LowerCaseToken, to: CamelCaseToken);

        private static void Rename(MigrationBuilder migrationBuilder, string from, string to)
        {
            foreach (var (table, columns) in InterpolatedColumns)
            {
                var set = string.Join(
                    ", ", columns.Select(x => $"[{x}] = REPLACE([{x}], N'{from}', N'{to}')"));

                /* No LIKE guard, unlike the other rename: under a case-insensitive collation it would
                   match both spellings and so filter nothing, while reading as though it does. The
                   UPDATE is correct either way - a case-sensitive collation replaces only the camelCase
                   spelling, a case-insensitive one also rewrites correct rows with identical text. */
                migrationBuilder.Sql($"UPDATE [{table}] SET {set};");
            }
        }
    }
}
