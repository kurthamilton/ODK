using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Topics;

namespace ODK.Data.EntityFramework.Mapping;

public class TopicGroupMap : IEntityTypeConfiguration<TopicGroup>
{
    public void Configure(EntityTypeBuilder<TopicGroup> builder)
    {
        builder.ToTable("TopicGroups");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("TopicGroupId");
    }
}
