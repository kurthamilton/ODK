using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Venues;
using ODK.Data.EntityFramework.Converters;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Mapping;

public class EventMap : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100);

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.DualWriteColumn(x => x.DescriptionHtml, writesTo: "DescriptionHtml", mirrorsTo: "Description");

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(1024);

        builder.Property(x => x.Name)
            .HasMaxLength(255);

        builder.Property(x => x.PublishedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.RsvpDeadlineUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.Shortcode)
            .HasMaxLength(255);

        builder.Property(x => x.Time)
            .HasMaxLength(255);

        builder.HasOne<Venue>()
            .WithMany()
            .HasForeignKey(x => x.VenueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TicketSettings)
            .WithOne()
            .HasForeignKey<EventTicketSettings>(x => x.EventId);

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
