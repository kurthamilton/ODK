using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Topics;

namespace ODK.Data.EntityFramework.Mapping;

public class TopicMap : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.ToTable("Topics");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.TopicGroup)
            .WithMany()
            .HasForeignKey(x => x.TopicGroupId);
    }
}
