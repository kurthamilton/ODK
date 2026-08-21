using System.Globalization;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class CurrenciesNameMakeRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var currencyNames = GetCurrencyNames();

            foreach (var currencyCode in currencyNames.Keys)
            {
                var name = currencyNames[currencyCode];

                var sql =
                    "UPDATE Currencies " +
                    "SET Name = '" + name.Replace("'", "''") + "' " +
                    "WHERE Code = '" + currencyCode + "'";
                migrationBuilder.Sql(sql);
            }

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Currencies",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Currencies",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);
        }

        private static IReadOnlyDictionary<string, string> GetCurrencyNames()
        {
            var regions = CultureInfo
                .GetCultures(CultureTypes.SpecificCultures)
                .Select(TryGetRegion)
                .Where(x => x != null)
                .Select(x => x!)
                .Where(x => IsCurrencyCode(x.ISOCurrencySymbol) && !string.IsNullOrEmpty(x.CurrencyEnglishName));

            return regions
                .GroupBy(x => x.ISOCurrencySymbol, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.First().CurrencyEnglishName, StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsCurrencyCode(string value)
            => value.Length == 3 && value.All(char.IsAsciiLetter);

        private static RegionInfo? TryGetRegion(CultureInfo culture)
        {
            try
            {
                return new RegionInfo(culture.Name);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}
