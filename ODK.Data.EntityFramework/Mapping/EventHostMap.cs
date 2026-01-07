using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Events;

namespace ODK.Data.EntityFramework.Mapping;

public class EventHostMap : IEntityTypeConfiguration<EventHost>
{
    public void Configure(EntityTypeBuilder<EventHost> builder)
    {
        builder.ToTable("EventHosts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("EventHostId");

        builder.HasOne<Event>()
            .WithMany()
            .HasForeignKey(x => x.EventId);

        builder.HasOne(x => x.Member)
            .WithMany()
            .HasForeignKey(x => x.MemberId);
    }
}

