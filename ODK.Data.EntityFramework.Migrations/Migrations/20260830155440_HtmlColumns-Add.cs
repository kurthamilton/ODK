using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class HtmlColumnsAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescriptionHtml",
                table: "SiteSubscriptions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnswerHtml",
                table: "SiteQuestions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MessageHtml",
                table: "SiteContactMessageReplies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionHtml",
                table: "ReferralCampaigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailTextHtml",
                table: "ReferralCampaigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BodyHtml",
                table: "QueuedEmails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionHtml",
                table: "Features",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionHtml",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BodyHtml",
                table: "Emails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionHtml",
                table: "ChapterTexts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegisterTextHtml",
                table: "ChapterTexts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WelcomeTextHtml",
                table: "ChapterTexts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionHtml",
                table: "ChapterSubscriptions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnswerHtml",
                table: "ChapterQuestions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultDescriptionHtml",
                table: "ChapterEventSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BodyHtml",
                table: "ChapterEmails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MessageHtml",
                table: "ChapterContactMessageReplies",
                type: "nvarchar(max)",
                nullable: true);

            /* The columns the app is still reading, copied across. It writes both from here on, so a
               row edited after this point keeps the two in step - except in the minute between this
               migration and the deploy that starts writing both, which the next migration reconciles. */
            migrationBuilder.Sql("UPDATE [SiteSubscriptions] SET [DescriptionHtml] = [Description]");
            migrationBuilder.Sql("UPDATE [SiteQuestions] SET [AnswerHtml] = [Answer]");
            migrationBuilder.Sql("UPDATE [SiteContactMessageReplies] SET [MessageHtml] = [Message]");
            migrationBuilder.Sql("UPDATE [ReferralCampaigns] SET [DescriptionHtml] = [Description]");
            migrationBuilder.Sql("UPDATE [ReferralCampaigns] SET [EmailTextHtml] = [EmailText]");
            migrationBuilder.Sql("UPDATE [QueuedEmails] SET [BodyHtml] = [Body]");
            migrationBuilder.Sql("UPDATE [Features] SET [DescriptionHtml] = [Description]");
            migrationBuilder.Sql("UPDATE [Events] SET [DescriptionHtml] = [Description]");
            migrationBuilder.Sql("UPDATE [Emails] SET [BodyHtml] = [Body]");
            migrationBuilder.Sql("UPDATE [ChapterTexts] SET [DescriptionHtml] = [Description]");
            migrationBuilder.Sql("UPDATE [ChapterTexts] SET [RegisterTextHtml] = [RegisterText]");
            migrationBuilder.Sql("UPDATE [ChapterTexts] SET [WelcomeTextHtml] = [WelcomeText]");
            migrationBuilder.Sql("UPDATE [ChapterSubscriptions] SET [DescriptionHtml] = [Description]");
            migrationBuilder.Sql("UPDATE [ChapterQuestions] SET [AnswerHtml] = [Answer]");
            migrationBuilder.Sql("UPDATE [ChapterEventSettings] SET [DefaultDescriptionHtml] = [DefaultDescription]");
            migrationBuilder.Sql("UPDATE [ChapterEmails] SET [BodyHtml] = [HtmlContent]");
            migrationBuilder.Sql("UPDATE [ChapterContactMessageReplies] SET [MessageHtml] = [Message]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Lossless: every value dropped here is still held by the column it was copied from.
            migrationBuilder.DropColumn(
                name: "DescriptionHtml",
                table: "SiteSubscriptions");

            migrationBuilder.DropColumn(
                name: "AnswerHtml",
                table: "SiteQuestions");

            migrationBuilder.DropColumn(
                name: "MessageHtml",
                table: "SiteContactMessageReplies");

            migrationBuilder.DropColumn(
                name: "DescriptionHtml",
                table: "ReferralCampaigns");

            migrationBuilder.DropColumn(
                name: "EmailTextHtml",
                table: "ReferralCampaigns");

            migrationBuilder.DropColumn(
                name: "BodyHtml",
                table: "QueuedEmails");

            migrationBuilder.DropColumn(
                name: "DescriptionHtml",
                table: "Features");

            migrationBuilder.DropColumn(
                name: "DescriptionHtml",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "BodyHtml",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "DescriptionHtml",
                table: "ChapterTexts");

            migrationBuilder.DropColumn(
                name: "RegisterTextHtml",
                table: "ChapterTexts");

            migrationBuilder.DropColumn(
                name: "WelcomeTextHtml",
                table: "ChapterTexts");

            migrationBuilder.DropColumn(
                name: "DescriptionHtml",
                table: "ChapterSubscriptions");

            migrationBuilder.DropColumn(
                name: "AnswerHtml",
                table: "ChapterQuestions");

            migrationBuilder.DropColumn(
                name: "DefaultDescriptionHtml",
                table: "ChapterEventSettings");

            migrationBuilder.DropColumn(
                name: "BodyHtml",
                table: "ChapterEmails");

            migrationBuilder.DropColumn(
                name: "MessageHtml",
                table: "ChapterContactMessageReplies");
        }
    }
}
