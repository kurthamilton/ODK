using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterPageMap : IEntityTypeConfiguration<ChapterPage>
{
    public void Configure(EntityTypeBuilder<ChapterPage> builder)
    {
        builder.ToTable("ChapterPages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("ChapterPageId");

        builder.Property(x => x.PageType)
            .HasColumnName("PageTypeId")
            .HasConversion<int>();

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId);
    }
}