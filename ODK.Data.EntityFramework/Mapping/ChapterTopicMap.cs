using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Core.Topics;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterTopicMap : IEntityTypeConfiguration<ChapterTopic>
{
    public void Configure(EntityTypeBuilder<ChapterTopic> builder)
    {
        builder.ToTable("ChapterTopics");

        builder.HasKey(x => new { x.ChapterId, x.TopicId });

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId);

        builder.HasOne<Topic>()
            .WithMany()
            .HasForeignKey(x => x.TopicId);
    }
}
