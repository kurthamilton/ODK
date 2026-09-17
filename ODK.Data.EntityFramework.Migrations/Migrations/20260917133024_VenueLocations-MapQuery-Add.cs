using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class VenueLocationsMapQueryAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MapQuery",
                table: "VenueLocations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            var sql =
                "UPDATE VenueLocations " +
                "SET VenueLocations.MapQuery = Venues.MapQuery " +
                "FROM Venues " +
                "JOIN VenueLocations ON Venues.Id = VenueLocations.VenueId ";
            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MapQuery",
                table: "VenueLocations");
        }
    }
}
