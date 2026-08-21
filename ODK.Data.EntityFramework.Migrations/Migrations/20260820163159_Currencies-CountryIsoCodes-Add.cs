using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class CurrenciesCountryIsoCodesAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CountryIsoCode2",
                table: "Currencies",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryIsoCode3",
                table: "Currencies",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            /* The columns stand in for the country a currency belongs to where no row in Countries claims
               it, which is the euro alone - every eurozone country references it, so the join that answers
               the question for every other currency has no one answer here. EU/EUU are the ISO 3166-1
               codes reserved for the European Union, and flagcdn serves its flag under "eu". */
            migrationBuilder.Sql(
                """
                UPDATE Currencies
                SET CountryIsoCode2 = 'EU',
                    CountryIsoCode3 = 'EUU'
                WHERE Code = 'EUR';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountryIsoCode2",
                table: "Currencies");

            migrationBuilder.DropColumn(
                name: "CountryIsoCode3",
                table: "Currencies");
        }
    }
}
