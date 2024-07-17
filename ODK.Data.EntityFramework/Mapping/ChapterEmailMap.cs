using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Emails;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterEmailMap : IEntityTypeConfiguration<ChapterEmail>
{
    public void Configure(EntityTypeBuilder<ChapterEmail> builder)
    {
        builder.ToTable("ChapterEmails");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("ChapterEmailId");
        builder.Property(x => x.Type)
            .HasColumnName("EmailTypeId")
            .HasConversion<int>();
    }
}
