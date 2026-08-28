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
            var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                RegionInfo region;

                try
                {
                    region = new RegionInfo(culture.Name);
                }
                catch (ArgumentException)
                {
                    // A culture with no region of its own names no currency.
                    continue;
                }

                if (IsCurrencyCode(region.ISOCurrencySymbol) && !string.IsNullOrEmpty(region.CurrencyEnglishName))
                {
                    names.TryAdd(region.ISOCurrencySymbol, region.CurrencyEnglishName);
                }
            }

            return names;
        }

        private static bool IsCurrencyCode(string value)
            => value.Length == 3 && value.All(char.IsAsciiLetter);
    }
}
