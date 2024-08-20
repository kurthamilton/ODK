using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Venues;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class VenueLocationMap : IEntityTypeConfiguration<VenueLocation>
{
    public void Configure(EntityTypeBuilder<VenueLocation> builder)
    {
        builder.ToTable("VenueLocations");

        builder.HasKey(x => x.VenueId);

        builder.Property(x => x.LatLong)
            .HasConversion<LatLongConverter>();

        builder.HasOne<Venue>()
            .WithOne()
            .HasForeignKey<VenueLocation>(x => x.VenueId);
    }
}
