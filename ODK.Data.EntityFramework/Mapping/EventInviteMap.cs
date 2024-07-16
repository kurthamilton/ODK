using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Events;

namespace ODK.Data.EntityFramework.Mapping;

public class EventInviteMap : IEntityTypeConfiguration<EventInvite>
{
    public void Configure(EntityTypeBuilder<EventInvite> builder)
    {
        builder.ToTable("EventInvites");

        builder.HasKey(x => new { x.EventId, x.MemberId });
    }
}
