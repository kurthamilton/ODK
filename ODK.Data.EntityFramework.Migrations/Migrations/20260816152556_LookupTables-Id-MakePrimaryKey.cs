using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class LookupTablesIdMakePrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* Fill Id for anything the previous migration missed: it backfilled when it was applied, and the
               build that was live for the rest of that deploy knew only the old column. EXEC because the
               generated script batches statements together - see LookupTables-Id-Add. */
            migrationBuilder.Sql("EXEC('UPDATE Topics SET Id = TopicId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE TopicGroups SET Id = TopicGroupId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Features SET Id = FeatureId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Currencies SET Id = CurrencyId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Countries SET Id = CountryId WHERE Id IS NULL')");

            /* Dropped by the column each sits on rather than by name - see ForeignKeySql. Several of these
               were created by hand and are not named the way EF would name them, and the scaffolder can only
               ever emit the name it would have chosen itself, so the drops it generates fail. They are added
               back below under EF's names, so the schema converges on the convention.

               Chapters.CountryId is in the list although the scaffolder does not drop it at all: the mapping
               for that relationship was added while this migration was being written, so EF sees only a
               foreign key to create and not the one already on the column. */
            migrationBuilder.DropForeignKeys("ChapterPaymentSettings", "CurrencyId");

            migrationBuilder.DropForeignKeys("ChapterSubscriptions", "CurrencyId");

            migrationBuilder.DropForeignKeys("ChapterTopics", "TopicId");

            migrationBuilder.DropForeignKeys("Countries", "CurrencyId");

            migrationBuilder.DropForeignKeys("EventTicketSettings", "CurrencyId");

            migrationBuilder.DropForeignKeys("EventTopics", "TopicId");

            migrationBuilder.DropForeignKeys("FeatureSeenByMembers", "FeatureId");

            migrationBuilder.DropForeignKeys("MemberLocations", "CountryId");

            migrationBuilder.DropForeignKeys("MemberPaymentSettings", "CurrencyId");

            migrationBuilder.DropForeignKeys("MemberTopics", "TopicId");

            migrationBuilder.DropForeignKeys("Payments", "CurrencyId");

            migrationBuilder.DropForeignKeys("SiteSubscriptionPrices", "CurrencyId");

            migrationBuilder.DropForeignKeys("Topics", "TopicGroupId");
            migrationBuilder.DropForeignKeys("Chapters", "CountryId");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Topics",
                table: "Topics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TopicGroups",
                table: "TopicGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Features",
                table: "Features");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Currencies",
                table: "Currencies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Countries",
                table: "Countries");

            /* Kept, not dropped - that is LookupTables-Id-Remove's job. The build that is live while this runs
               still reads and writes the old column, so it has to survive until the build after this one has
               replaced it. Nullable because that build no longer writes it. */
            migrationBuilder.AlterColumn<Guid>(
                name: "TopicId",
                table: "Topics",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "TopicGroupId",
                table: "TopicGroups",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "FeatureId",
                table: "Features",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "CurrencyId",
                table: "Currencies",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "CountryId",
                table: "Countries",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Topics",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "TopicGroups",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Features",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Currencies",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Countries",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Topics",
                table: "Topics",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TopicGroups",
                table: "TopicGroups",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Features",
                table: "Features",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Currencies",
                table: "Currencies",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Countries",
                table: "Countries",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_CountryId",
                table: "Chapters",
                column: "CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterPaymentSettings_Currencies_CurrencyId",
                table: "ChapterPaymentSettings",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chapters_Countries_CountryId",
                table: "Chapters",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterSubscriptions_Currencies_CurrencyId",
                table: "ChapterSubscriptions",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterTopics_Topics_TopicId",
                table: "ChapterTopics",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Countries_Currencies_CurrencyId",
                table: "Countries",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventTicketSettings_Currencies_CurrencyId",
                table: "EventTicketSettings",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventTopics_Topics_TopicId",
                table: "EventTopics",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FeatureSeenByMembers_Features_FeatureId",
                table: "FeatureSeenByMembers",
                column: "FeatureId",
                principalTable: "Features",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberLocations_Countries_CountryId",
                table: "MemberLocations",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberPaymentSettings_Currencies_CurrencyId",
                table: "MemberPaymentSettings",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberTopics_Topics_TopicId",
                table: "MemberTopics",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Currencies_CurrencyId",
                table: "Payments",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteSubscriptionPrices_Currencies_CurrencyId",
                table: "SiteSubscriptionPrices",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Topics_TopicGroups_TopicGroupId",
                table: "Topics",
                column: "TopicGroupId",
                principalTable: "TopicGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChapterPaymentSettings_Currencies_CurrencyId",
                table: "ChapterPaymentSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_Chapters_Countries_CountryId",
                table: "Chapters");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterSubscriptions_Currencies_CurrencyId",
                table: "ChapterSubscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterTopics_Topics_TopicId",
                table: "ChapterTopics");

            migrationBuilder.DropForeignKey(
                name: "FK_Countries_Currencies_CurrencyId",
                table: "Countries");

            migrationBuilder.DropForeignKey(
                name: "FK_EventTicketSettings_Currencies_CurrencyId",
                table: "EventTicketSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_EventTopics_Topics_TopicId",
                table: "EventTopics");

            migrationBuilder.DropForeignKey(
                name: "FK_FeatureSeenByMembers_Features_FeatureId",
                table: "FeatureSeenByMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberLocations_Countries_CountryId",
                table: "MemberLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberPaymentSettings_Currencies_CurrencyId",
                table: "MemberPaymentSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberTopics_Topics_TopicId",
                table: "MemberTopics");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Currencies_CurrencyId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_SiteSubscriptionPrices_Currencies_CurrencyId",
                table: "SiteSubscriptionPrices");

            migrationBuilder.DropForeignKey(
                name: "FK_Topics_TopicGroups_TopicGroupId",
                table: "Topics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Topics",
                table: "Topics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TopicGroups",
                table: "TopicGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Features",
                table: "Features");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Currencies",
                table: "Currencies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Countries",
                table: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_Chapters_CountryId",
                table: "Chapters");

            /* The old columns were never dropped, so going back restores rather than recreates them. Rows
               written while Id was the key left them unset, so fill those before making them required. */
            migrationBuilder.Sql("EXEC('UPDATE Topics SET TopicId = Id WHERE TopicId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE TopicGroups SET TopicGroupId = Id WHERE TopicGroupId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Features SET FeatureId = Id WHERE FeatureId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Currencies SET CurrencyId = Id WHERE CurrencyId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Countries SET CountryId = Id WHERE CountryId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "TopicId",
                table: "Topics",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TopicGroupId",
                table: "TopicGroups",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "FeatureId",
                table: "Features",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CurrencyId",
                table: "Currencies",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CountryId",
                table: "Countries",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Topics",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "TopicGroups",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Features",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Currencies",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Countries",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Topics",
                table: "Topics",
                column: "TopicId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TopicGroups",
                table: "TopicGroups",
                column: "TopicGroupId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Features",
                table: "Features",
                column: "FeatureId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Currencies",
                table: "Currencies",
                column: "CurrencyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Countries",
                table: "Countries",
                column: "CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterPaymentSettings_Currencies_CurrencyId",
                table: "ChapterPaymentSettings",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "CurrencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterSubscriptions_Currencies_CurrencyId",
                table: "ChapterSubscriptions",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "CurrencyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterTopics_Topics_TopicId",
                table: "ChapterTopics",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "TopicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Countries_Currencies_CurrencyId",
                table: "Countries",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "CurrencyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventTicketSettings_Currencies_CurrencyId",
                table: "EventTicketSettings",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "CurrencyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventTopics_Topics_TopicId",
                table: "EventTopics",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "TopicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FeatureSeenByMembers_Features_FeatureId",
                table: "FeatureSeenByMembers",
                column: "FeatureId",
                principalTable: "Features",
                principalColumn: "FeatureId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberLocations_Countries_CountryId",
                table: "MemberLocations",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberPaymentSettings_Currencies_CurrencyId",
                table: "MemberPaymentSettings",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "CurrencyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberTopics_Topics_TopicId",
                table: "MemberTopics",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "TopicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Currencies_CurrencyId",
                table: "Payments",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "CurrencyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteSubscriptionPrices_Currencies_CurrencyId",
                table: "SiteSubscriptionPrices",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "CurrencyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Topics_TopicGroups_TopicGroupId",
                table: "Topics",
                column: "TopicGroupId",
                principalTable: "TopicGroups",
                principalColumn: "TopicGroupId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
