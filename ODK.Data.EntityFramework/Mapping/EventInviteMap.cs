using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Events;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class EventInviteMap : IEntityTypeConfiguration<EventInvite>
{
    public void Configure(EntityTypeBuilder<EventInvite> builder)
    {
        builder.ToTable("EventInvites");

        builder.HasKey(x => new { x.EventId, x.MemberId });

        builder.Property(x => x.SentUtc)
            .HasColumnName("SentDate")
            .HasConversion<UtcDateTimeConverter>();
    }
}
