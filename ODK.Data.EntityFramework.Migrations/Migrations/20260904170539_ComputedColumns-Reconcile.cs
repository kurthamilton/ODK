using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ComputedColumnsReconcile : Migration
    {
        /* No schema change. Six columns are computed in the database - VersionInt on ChapterImages,
           MemberAvatars and InstagramImages, LatLong on ChapterLocations, MemberLocations and
           VenueLocations - and the model has stopped describing them as ordinary ones. A model the
           migrations do not account for is refused at the point they are applied, so the change is
           recorded here to keep the deploy that ships it applicable.

           The columns themselves are corrected in InitialCreate, which is where a database built from the
           migrations gets them, and which no existing database re-runs. Scaffolding wrote six AlterColumn
           calls instead; they are deleted deliberately. SQL Server turns an ordinary column into a computed
           one by dropping and re-adding it, so against every database that already has these columns right
           they would rebuild three geography columns and three int ones to arrive back where they started. */

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
