using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterSubscriptionMap : IEntityTypeConfiguration<ChapterSubscription>
{
    public void Configure(EntityTypeBuilder<ChapterSubscription> builder)
    {
        builder.ToTable("ChapterSubscriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("ChapterSubscriptionId");

        builder.Property(x => x.Type)
            .HasColumnName("SubscriptionTypeId")
            .HasConversion<int>();
    }
}
