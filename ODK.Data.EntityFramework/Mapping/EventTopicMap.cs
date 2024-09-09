using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Events;
using ODK.Core.Topics;

namespace ODK.Data.EntityFramework.Mapping;

public class EventTopicMap : IEntityTypeConfiguration<EventTopic>
{
    public void Configure(EntityTypeBuilder<EventTopic> builder)
    {
        builder.ToTable("EventTopics");

        builder.HasKey(x => new { x.EventId, x.TopicId });

        builder.HasOne<Event>()
            .WithMany()
            .HasForeignKey(x => x.EventId);

        builder.HasOne<Topic>()
            .WithMany()
            .HasForeignKey(x => x.TopicId);
    }
}
