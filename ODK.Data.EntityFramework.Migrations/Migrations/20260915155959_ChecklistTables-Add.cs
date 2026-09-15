using System;
using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Chapters;
using ODK.Data.EntityFramework.Migrations.Enums;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ChecklistTablesAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .CreateEnumTable<ChecklistItemType>()
                .InsertAllEnumValues<ChecklistItemType>();

            migrationBuilder.CreateTable(
                name: "ChecklistItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Dismissable = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ChecklistItemTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistItems", x => x.Id);
                    table.UniqueConstraint("AK_ChecklistItems_ChecklistItemTypeId", x => x.ChecklistItemTypeId);
                });

            migrationBuilder
                .AddEnumForeignKey<ChecklistItemType>(
                    table: "ChecklistItems",
                    column: "ChecklistItemTypeId");

            /* The checklist itself. Both tables are new everywhere, so these inserts need none of the
               guarding the enum lookup tables carry - there is no restored database that already has them.
               The ids are literals because a migration has to produce the same rows every time it runs.
               DisplayOrder is seeded to match the step's number because that is the order they were
               written in; it is a separate column so a later step can be slotted between two existing
               ones. */
            migrationBuilder.InsertData(
                table: "ChecklistItems",
                columns: ["Id", "ChecklistItemTypeId", "Dismissable", "DisplayOrder", "Name"],
                values: new object[,]
                {
                    { new Guid("d7277aab-06c8-4c48-a03d-928297a49b22"), 1, false, 1, "CreateGroup" },
                    { new Guid("7c145a0a-e8ba-4b50-894d-0888e3b421f3"), 2, false, 2, "Picture" },
                    { new Guid("8be2cdb9-5b7d-425c-a137-85db94808ea6"), 3, false, 3, "Description" },
                    { new Guid("72b70f46-9491-41eb-b2da-7bcc0fb048d9"), 4, false, 4, "MembershipSettings" },
                    { new Guid("8297503c-726d-4677-8f08-f2cf9e075173"), 5, false, 5, "PrivacySettings" },
                    { new Guid("d037b9dd-51e1-43cb-88cd-0cde068f3b53"), 6, true, 6, "Questions" },
                    { new Guid("8800f533-e60d-4451-a459-c0b9b1010889"), 7, true, 7, "MemberProperties" },
                    { new Guid("799be4d7-8a83-4dba-9381-9e7ec06fc36f"), 8, true, 8, "Topics" },
                    { new Guid("e5d16daa-b19a-484f-b857-c6b0f7ff06cb"), 9, false, 9, "SubmitForApproval" },
                    { new Guid("a3ac7f18-151c-4b28-a280-f1bcc85ea9b4"), 10, false, 10, "Publish" },
                    { new Guid("3a07d62d-d1da-4484-a8c2-224907033265"), 11, false, 11, "FirstEvent" }
                });

            migrationBuilder.CreateTable(
                name: "ChapterChecklistItems",
                columns: table => new
                {
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChecklistItemTypeId = table.Column<int>(type: "int", nullable: false),
                    CompletedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DismissedUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChapterChecklistItems", x => new { x.ChapterId, x.ChecklistItemTypeId })
                        .Annotation("SqlServer:Clustered", true);
                    table.ForeignKey(
                        name: "FK_ChapterChecklistItems_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChapterChecklistItems_ChecklistItems_ChecklistItemTypeId",
                        column: x => x.ChecklistItemTypeId,
                        principalTable: "ChecklistItems",
                        principalColumn: "ChecklistItemTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder
                .AddEnumForeignKey<ChecklistItemType>(
                    table: "ChapterChecklistItems",
                    column: "ChecklistItemTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChapterChecklistItems_ChecklistItemTypeId",
                table: "ChapterChecklistItems",
                column: "ChecklistItemTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .DropEnumForeignKey<ChecklistItemType>(
                    table: "ChecklistItems",
                    column: "ChecklistItemTypeId");

            migrationBuilder
                .DropEnumForeignKey<ChecklistItemType>(
                    table: "ChapterChecklistItems",
                    column: "ChecklistItemTypeId");

            migrationBuilder.DropEnumTable<ChecklistItemType>();

            migrationBuilder.DropTable(
                name: "ChapterChecklistItems");

            migrationBuilder.DropTable(
                name: "ChecklistItems");
        }
    }
}
