using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Events;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class EventEmailMap : IEntityTypeConfiguration<EventEmail>
{
    public void Configure(EntityTypeBuilder<EventEmail> builder)
    {
        builder.ToTable("EventEmails");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("EventEmailId");

        builder.Property(x => x.ScheduledUtc)
            .HasColumnName("ScheduledDate")
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.SentUtc)
            .HasColumnName("SentDate")
            .HasConversion<UtcDateTimeConverter>();

        builder.HasOne<Event>()
            .WithOne()
            .HasForeignKey<EventEmail>(x => x.EventId);
    }
}
