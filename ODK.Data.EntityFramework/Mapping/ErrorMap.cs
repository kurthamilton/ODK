using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Logging;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class ErrorMap : IEntityTypeConfiguration<Error>
{
    public void Configure(EntityTypeBuilder<Error> builder)
    {
        builder.ToTable("Errors");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        // Transition shadow keeps the legacy CreatedDate column populated until it is dropped.
        builder.Property<DateTime?>("CreatedUtcColumn")
            .HasColumnName("CreatedDate")
            .HasConversion<NullableUtcDateTimeConverter>();
    }
}
