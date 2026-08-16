using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterPropertyOptionMap : IEntityTypeConfiguration<ChapterPropertyOption>
{
    public void Configure(EntityTypeBuilder<ChapterPropertyOption> builder)
    {
        builder.ToTable("ChapterPropertyOptions");

        builder.HasKey(x => x.Id);

        builder.HasOne<ChapterProperty>()
            .WithMany()
            .HasForeignKey(x => x.ChapterPropertyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}