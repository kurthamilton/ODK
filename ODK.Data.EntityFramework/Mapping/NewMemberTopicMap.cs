using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;
using ODK.Core.Topics;

namespace ODK.Data.EntityFramework.Mapping;

public class NewMemberTopicMap : IEntityTypeConfiguration<NewMemberTopic>
{
    public void Configure(EntityTypeBuilder<NewMemberTopic> builder)
    {
        builder.ToTable("NewMemberTopics");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("NewMemberTopicId");

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
