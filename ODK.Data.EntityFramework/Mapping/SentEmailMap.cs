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

        builder.Property(x => x.Id)
            .HasColumnName("SentEmailId");

        builder.Property(x => x.SentUtc)
            .HasConversion<UtcDateTimeConverter>();

        // Transition shadow keeps the legacy SentDate column populated until it is dropped.
        builder.Property<DateTime?>("SentUtcColumn")
            .HasColumnName("SentDate")
            .HasConversion<NullableUtcDateTimeConverter>();
    }
}
