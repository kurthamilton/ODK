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

        builder.Property(x => x.ExternalId)
            .HasMaxLength(VenueLocation.ExternalIdMaxLength);

        /* Not unique: a place keeps its ID while its name or position change, and each of those is its own
           venue with its own location. Indexed because resolving a submitted place to the venue recording
           it is the hot path. */
        builder.HasIndex(x => x.ExternalId);

        /* Shadow property mapped to the LatLong column to enable server-side spatial queries. The
           database derives the point from the two coordinates, so nothing writes it. */
        builder.Property<Point>("LatLongPoint")
            .HasColumnName("LatLong")
            .HasColumnType("geography")
            .HasComputedColumnSql("[geography]::Point([Latitude],[Longitude],(4326))", stored: true);

        builder.HasOne<Venue>()
            .WithOne()
            .HasForeignKey<VenueLocation>(x => x.VenueId);
    }
}