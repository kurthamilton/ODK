using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Core.Venues;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class VenueMap : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable("Venues");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Address)
            .HasMaxLength(255);

        builder.Property(x => x.ArchivedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.MapQuery)
            .HasMaxLength(255);

        builder.Property(x => x.Name)
            .HasMaxLength(255);

        builder.Property(x => x.Slug)
            .HasMaxLength(Venue.SlugMaxLength);

        builder.Property(x => x.Version)
            .IsRowVersion();

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.ChapterId, x.Name })
            .IsUnique();

        builder.HasIndex(x => new { x.ChapterId, x.Slug })
            .IsUnique();
    }
}