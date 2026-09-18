using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Venues;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class VenueMap : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable("Venues");

        /* A venue is reached by its key: ChapterVenues is clustered on (ChapterId, VenueId) and yields
           venue ids, and every other read is a lookup by id. */
        builder.HasKey(x => x.Id)
            .IsClustered();

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.Name)
            .HasMaxLength(Venue.NameMaxLength);

        builder.Property(x => x.Slug)
            .HasMaxLength(Venue.SlugMaxLength);

        builder.HasIndex(x => x.Slug)
            .IsUnique();
    }
}