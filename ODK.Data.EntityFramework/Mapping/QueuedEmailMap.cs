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

        builder.Property(x => x.Id)
            .HasColumnName("QueuedEmailId");

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.SendAfterUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId);
    }
}
