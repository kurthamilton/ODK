using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Core.Venues;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterVenueMap : IEntityTypeConfiguration<ChapterVenue>
{
    public void Configure(EntityTypeBuilder<ChapterVenue> builder)
    {
        builder.ToTable("ChapterVenues");

        builder.HasIndex(x => x.Id);

        /* Every read is "this chapter's venues", so the key leads with ChapterId and the rows a chapter
           owns sit together under the clustered key. The reverse direction - which chapters use a venue -
           is served by the index on the VenueId foreign key. */
        builder.HasIndex(x => new { x.ChapterId, x.VenueId })
            .IsUnique();

        builder.Property(x => x.ArchivedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.AdditionalInfo)
            .HasMaxLength(ChapterVenue.AdditionalInfoMaxLength);

        builder.Property(x => x.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(x => x.Name)
            .HasMaxLength(Venue.NameMaxLength);

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Venue)
            .WithMany()
            .HasForeignKey(x => x.VenueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
