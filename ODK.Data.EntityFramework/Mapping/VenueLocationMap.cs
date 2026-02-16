using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;
using ODK.Core.Venues;

namespace ODK.Data.EntityFramework.Mapping;

public class VenueLocationMap : IEntityTypeConfiguration<VenueLocation>
{
    public void Configure(EntityTypeBuilder<VenueLocation> builder)
    {
        builder.ToTable("VenueLocations");

        builder.HasKey(x => x.VenueId);

        // Shadow property mapped to the LatLong column to enable server-side spatial queries
        builder.Property<Point>("LatLongPoint")
            .HasColumnName("LatLong")
            .HasColumnType("geography")
            .ValueGeneratedOnAddOrUpdate();

        builder.HasOne<Venue>()
            .WithOne()
            .HasForeignKey<VenueLocation>(x => x.VenueId);
    }
}