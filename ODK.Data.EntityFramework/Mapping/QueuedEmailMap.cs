using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class QueuedEmailMap : IEntityTypeConfiguration<QueuedEmail>
{
    public void Configure(EntityTypeBuilder<QueuedEmail> builder)
    {
        builder.ToTable("QueuedEmails");

        builder.HasKey(x => x.Id);

        // Pinned to the current column name - the columns are renamed to match in a later migration.
        builder.Property(x => x.BodyHtml)
            .HasColumnName("Body");

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.SendAfterUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        // Stated rather than left to convention: the column is optional, which would give no action, while
        // the database cascades - a queued email has no meaning once its group is gone.
        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
