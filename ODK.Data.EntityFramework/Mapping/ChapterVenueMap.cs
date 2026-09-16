using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Core.Venues;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterVenueMap : IEntityTypeConfiguration<ChapterVenue>
{
    public void Configure(EntityTypeBuilder<ChapterVenue> builder)
    {
        builder.ToTable("ChapterVenues");

        /* Every read is "this chapter's venues", so the key leads with ChapterId and the rows a chapter
           owns sit together under the clustered key. The reverse direction - which chapters use a venue -
           is served by the index on the VenueId foreign key. */
        builder.HasKey(x => new { x.ChapterId, x.VenueId });

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Venue>()
            .WithMany()
            .HasForeignKey(x => x.VenueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
