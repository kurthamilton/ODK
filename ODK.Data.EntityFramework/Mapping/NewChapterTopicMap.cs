using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Topics;

namespace ODK.Data.EntityFramework.Mapping;

public class NewChapterTopicMap : IEntityTypeConfiguration<NewChapterTopic>
{
    public void Configure(EntityTypeBuilder<NewChapterTopic> builder)
    {
        builder.ToTable("NewChapterTopics");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("NewChapterTopicId");

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
