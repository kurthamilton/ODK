using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterPropertyMap : IEntityTypeConfiguration<ChapterProperty>
{
    public void Configure(EntityTypeBuilder<ChapterProperty> builder)
    {
        builder.ToTable("ChapterProperties");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApplicationOnly)
            .HasColumnName("Hidden");

        builder.Property(x => x.DataType)
            .HasColumnName("DataTypeId")
            .HasConversion<int>();

        builder.Property(x => x.DisplayName)
            .HasMaxLength(255);

        builder.Property(x => x.HelpText)
            .HasMaxLength(255);

        builder.Property(x => x.Label)
            .HasMaxLength(255);

        builder.Property(x => x.Name)
            .HasMaxLength(255);

        builder.Property(x => x.Subtitle)
            .HasMaxLength(255);

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}