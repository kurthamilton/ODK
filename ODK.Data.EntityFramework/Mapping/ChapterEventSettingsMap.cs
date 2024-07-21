using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterEventSettingsMap : IEntityTypeConfiguration<ChapterEventSettings>
{
    public void Configure(EntityTypeBuilder<ChapterEventSettings> builder)
    {
        builder.ToTable("ChapterEventSettings");

        builder.HasKey(x => x.ChapterId);
    }
}
