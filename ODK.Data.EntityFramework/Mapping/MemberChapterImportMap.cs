using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberChapterImportMap : IEntityTypeConfiguration<MemberChapterImport>
{
    public void Configure(EntityTypeBuilder<MemberChapterImport> builder)
    {
        builder.ToTable("MemberChapterImports");

        builder.HasKey(x => x.Id)
            .IsClustered(false);

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.EmailAddress)
            .HasMaxLength(255);

        builder.Property(x => x.FirstName)
            .HasMaxLength(50);

        builder.Property(x => x.LastName)
            .HasMaxLength(50);

        builder.Property(x => x.SourceFileName)
            .HasMaxLength(255);

        builder.Property(x => x.UploadedUtc)
            .HasConversion<UtcDateTimeConverter>();

        /* Every read is one group's rows, and an upload looks each address up within that group, so one
           index answers both. Unique because a group holds a single row per address: a later upload of the
           same address updates the row it already has. */
        builder.HasIndex(x => new { x.ChapterId, x.EmailAddress })
            .IsUnique()
            .IsClustered();

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
