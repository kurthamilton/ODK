using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Emails;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class SentEmailEventMap : IEntityTypeConfiguration<SentEmailEvent>
{
    public void Configure(EntityTypeBuilder<SentEmailEvent> builder)
    {
        builder.ToTable("SentEmailEvents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.Id)
            .HasColumnName("SentEmailEventId");

        builder.HasOne<SentEmail>()
            .WithMany()
            .HasForeignKey(x => x.SentEmailId);
    }
}
