using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterTextsMap : IEntityTypeConfiguration<ChapterTexts>
{
    public void Configure(EntityTypeBuilder<ChapterTexts> builder)
    {
        builder.ToTable("ChapterTexts");

        builder.HasKey(x => x.ChapterId);

        // Pinned to the current column name - the columns are renamed to match in a later migration.
        builder.Property(x => x.DescriptionHtml)
            .HasColumnName("Description");

        builder.Property(x => x.RegisterTextHtml)
            .HasColumnName("RegisterText");

        builder.Property(x => x.WelcomeTextHtml)
            .HasColumnName("WelcomeText");

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId);
    }
}
