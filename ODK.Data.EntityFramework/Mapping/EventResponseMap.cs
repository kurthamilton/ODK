using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Events;

namespace ODK.Data.EntityFramework.Mapping;

public class EventResponseMap : IEntityTypeConfiguration<EventResponse>
{
    public void Configure(EntityTypeBuilder<EventResponse> builder)
    {
        builder.ToTable("EventResponses");

        builder.HasKey(x => new { x.EventId, x.MemberId });

        builder.Property(x => x.Type)
            .HasColumnName("ResponseTypeId");
    }
}
