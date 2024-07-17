using System.Data;
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

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId);
    }
}
