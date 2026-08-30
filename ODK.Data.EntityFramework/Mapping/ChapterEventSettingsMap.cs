using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterEventSettingsMap : IEntityTypeConfiguration<ChapterEventSettings>
{
    public void Configure(EntityTypeBuilder<ChapterEventSettings> builder)
    {
        builder.ToTable("ChapterEventSettings");

        builder.HasKey(x => x.ChapterId);

        builder.DualWriteColumn(
            x => x.DefaultDescriptionHtml, writesTo: "DefaultDescription", mirrorsTo: "DefaultDescriptionHtml");

        builder.HasOne<Chapter>()
            .WithOne()
            .HasForeignKey<ChapterEventSettings>(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
