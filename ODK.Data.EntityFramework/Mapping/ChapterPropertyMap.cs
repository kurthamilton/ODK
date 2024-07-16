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

        builder.Property(x => x.Id)
            .HasColumnName("ChapterPropertyId");

        builder.Property(x => x.DataType)
            .HasColumnName("DataTypeId")
            .HasConversion<int>();
    }
}
