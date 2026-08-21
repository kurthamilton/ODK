using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class CurrenciesCountryNameAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CountryName",
                table: "Currencies",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            // Completes the country the euro carries itself, alongside the ISO codes added with it: the
            // country columns are read as a set, so a name left null here would fall through to whichever
            // eurozone country the Countries join returned - a name beside the European Union's flag.
            migrationBuilder.Sql(
                """
                UPDATE Currencies
                SET CountryName = 'European Union'
                WHERE Code = 'EUR';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountryName",
                table: "Currencies");
        }
    }
}
