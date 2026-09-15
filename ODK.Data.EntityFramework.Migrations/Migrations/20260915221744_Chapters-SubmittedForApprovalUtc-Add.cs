using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ChaptersSubmittedForApprovalUtcAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedForApprovalUtc",
                table: "Chapters",
                type: "datetime2",
                nullable: true);

            /* Approval is only legal from a submitted group, so a group approved before there was anything
               to submit has to carry a submission date or it reads as never having offered itself. Dated
               from its creation, which under the flow it was approved under is when it was offered.

               Only the approved ones: a group still waiting is now its owner's to submit, which is the
               whole point of the step. */
            migrationBuilder.Sql(
                """
                UPDATE [Chapters]
                SET [SubmittedForApprovalUtc] = [CreatedUtc]
                WHERE [ApprovedUtc] IS NOT NULL
                    AND [SubmittedForApprovalUtc] IS NULL
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubmittedForApprovalUtc",
                table: "Chapters");
        }
    }
}
