using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class EventWaitlistMemberMap : IEntityTypeConfiguration<EventWaitlistMember>
{
    public void Configure(EntityTypeBuilder<EventWaitlistMember> builder)
    {
        builder.ToTable("EventWaitlistMembers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.Id)
            .HasColumnName("EventWaitlistMemberId");

        builder.HasOne<Event>()
            .WithMany()
            .HasForeignKey(x => x.EventId);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId);
    }
}