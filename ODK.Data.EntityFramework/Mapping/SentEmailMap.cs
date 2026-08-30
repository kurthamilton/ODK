using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Emails;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class SentEmailMap : IEntityTypeConfiguration<SentEmail>
{
    public void Configure(EntityTypeBuilder<SentEmail> builder)
    {
        builder.ToTable("SentEmails");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ExternalId)
            .HasMaxLength(255);

        builder.Property(x => x.SentUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.Subject)
            .HasMaxLength(255);

        builder.Property(x => x.To)
            .HasMaxLength(255);
    }
}
