using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class HtmlColumnsDropOld : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* Last of five releases moving these seventeen columns to the Html suffix. The live build
               maps only the columns that replaced them, and HtmlColumns-Unmap took these out of the
               model, so nothing reads them - which is also why this is hand-written: with the columns
               already out of the snapshot there is nothing left for the scaffolder to notice.

               DropColumn, not DropColumnIfExists: InitialCreate created every one of them, so every
               database has them. DropColumn also clears the default constraint that would otherwise
               block the drop, which a restored database names after itself and nothing could drop by
               name. */
            migrationBuilder.DropColumn(
                name: "Description",
                table: "SiteSubscriptions");

            migrationBuilder.DropColumn(
                name: "Answer",
                table: "SiteQuestions");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "SiteContactMessageReplies");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ReferralCampaigns");

            migrationBuilder.DropColumn(
                name: "EmailText",
                table: "ReferralCampaigns");

            migrationBuilder.DropColumn(
                name: "Body",
                table: "QueuedEmails");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Features");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Body",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ChapterTexts");

            migrationBuilder.DropColumn(
                name: "RegisterText",
                table: "ChapterTexts");

            migrationBuilder.DropColumn(
                name: "WelcomeText",
                table: "ChapterTexts");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ChapterSubscriptions");

            migrationBuilder.DropColumn(
                name: "Answer",
                table: "ChapterQuestions");

            migrationBuilder.DropColumn(
                name: "DefaultDescription",
                table: "ChapterEventSettings");

            migrationBuilder.DropColumn(
                name: "HtmlContent",
                table: "ChapterEmails");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "ChapterContactMessageReplies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /* Lossless: the column that replaced each one still carries its value, so the restored
               column takes it from there. Optional, which is how each of them stood at the drop. */
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SiteSubscriptions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Answer",
                table: "SiteQuestions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "SiteContactMessageReplies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ReferralCampaigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailText",
                table: "ReferralCampaigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Body",
                table: "QueuedEmails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Features",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Body",
                table: "Emails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ChapterTexts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegisterText",
                table: "ChapterTexts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WelcomeText",
                table: "ChapterTexts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ChapterSubscriptions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Answer",
                table: "ChapterQuestions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultDescription",
                table: "ChapterEventSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HtmlContent",
                table: "ChapterEmails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "ChapterContactMessageReplies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql("UPDATE [SiteSubscriptions] SET [Description] = [DescriptionHtml]");
            migrationBuilder.Sql("UPDATE [SiteQuestions] SET [Answer] = [AnswerHtml]");
            migrationBuilder.Sql("UPDATE [SiteContactMessageReplies] SET [Message] = [MessageHtml]");
            migrationBuilder.Sql("UPDATE [ReferralCampaigns] SET [Description] = [DescriptionHtml]");
            migrationBuilder.Sql("UPDATE [ReferralCampaigns] SET [EmailText] = [EmailTextHtml]");
            migrationBuilder.Sql("UPDATE [QueuedEmails] SET [Body] = [BodyHtml]");
            migrationBuilder.Sql("UPDATE [Features] SET [Description] = [DescriptionHtml]");
            migrationBuilder.Sql("UPDATE [Events] SET [Description] = [DescriptionHtml]");
            migrationBuilder.Sql("UPDATE [Emails] SET [Body] = [BodyHtml]");
            migrationBuilder.Sql("UPDATE [ChapterTexts] SET [Description] = [DescriptionHtml]");
            migrationBuilder.Sql("UPDATE [ChapterTexts] SET [RegisterText] = [RegisterTextHtml]");
            migrationBuilder.Sql("UPDATE [ChapterTexts] SET [WelcomeText] = [WelcomeTextHtml]");
            migrationBuilder.Sql("UPDATE [ChapterSubscriptions] SET [Description] = [DescriptionHtml]");
            migrationBuilder.Sql("UPDATE [ChapterQuestions] SET [Answer] = [AnswerHtml]");
            migrationBuilder.Sql("UPDATE [ChapterEventSettings] SET [DefaultDescription] = [DefaultDescriptionHtml]");
            migrationBuilder.Sql("UPDATE [ChapterEmails] SET [HtmlContent] = [BodyHtml]");
            migrationBuilder.Sql("UPDATE [ChapterContactMessageReplies] SET [Message] = [MessageHtml]");
        }
    }
}
