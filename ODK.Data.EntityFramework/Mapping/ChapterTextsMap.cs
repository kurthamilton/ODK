using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterTextsMap : IEntityTypeConfiguration<ChapterTexts>
{
    public void Configure(EntityTypeBuilder<ChapterTexts> builder)
    {
        builder.ToTable("ChapterTexts");

        builder.HasKey(x => x.ChapterId);

        builder.DualWriteColumn(
            x => x.DescriptionHtml, writesTo: "DescriptionHtml", mirrorsTo: "Description");

        builder.DualWriteColumn(
            x => x.RegisterTextHtml, writesTo: "RegisterTextHtml", mirrorsTo: "RegisterText");

        builder.DualWriteColumn(
            x => x.WelcomeTextHtml, writesTo: "WelcomeTextHtml", mirrorsTo: "WelcomeText");

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId);
    }
}
