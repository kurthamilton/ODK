using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;
using ODK.Core.Topics;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberTopicMap : IEntityTypeConfiguration<MemberTopic>
{
    public void Configure(EntityTypeBuilder<MemberTopic> builder)
    {
        builder.ToTable("MemberTopics");

        builder.HasKey(x => new { x.MemberId, x.TopicId });

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId);

        builder.HasOne<Topic>()
            .WithMany()
            .HasForeignKey(x => x.TopicId);
    }
}
