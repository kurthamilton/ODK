using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Venues;

namespace ODK.Data.EntityFramework.Mapping;

public class VenueMap : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable("Venues");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("VenueId");

        builder.Property(x => x.Version)
            .IsRowVersion();
    }
}
