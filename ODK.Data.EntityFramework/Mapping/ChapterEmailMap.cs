using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Core.Emails;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterEmailMap : IEntityTypeConfiguration<ChapterEmail>
{
    public void Configure(EntityTypeBuilder<ChapterEmail> builder)
    {
        builder.ToTable("ChapterEmails");

        builder.HasKey(x => x.Id)
            .IsClustered(false);

        builder.HasIndex(x => x.ChapterId)
            .IsClustered();

        builder.Property(x => x.Subject)
            .HasMaxLength(255);

        builder.Property(x => x.Type)
            .HasColumnName("EmailTypeId")
            .HasConversion<int>();

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId);

        builder.HasOne<Email>()
            .WithMany()
            .HasForeignKey(x => x.Type);

        builder.HasIndex(x => new { x.ChapterId, x.Type })
            .IsUnique();
    }
}
