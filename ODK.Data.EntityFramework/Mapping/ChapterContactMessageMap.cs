using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterContactMessageMap : IEntityTypeConfiguration<ChapterContactMessage>
{
    public void Configure(EntityTypeBuilder<ChapterContactMessage> builder)
    {
        builder.ToTable("ChapterContactMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.Id)
            .HasColumnName("ChapterContactMessageId");

        builder.Property(x => x.RepliedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();
    }
}
