using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Events;
using ODK.Core.Members;

namespace ODK.Data.EntityFramework.Mapping;

public class EventResponseMap : IEntityTypeConfiguration<EventResponse>
{
    public void Configure(EntityTypeBuilder<EventResponse> builder)
    {
        builder.ToTable("EventResponses");

        builder.HasKey(x => new { x.EventId, x.MemberId });

        builder.Property(x => x.Type)
            .HasColumnName("ResponseTypeId");

        builder.HasOne<Event>()
            .WithMany()
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}