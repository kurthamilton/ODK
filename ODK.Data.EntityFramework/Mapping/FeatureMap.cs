using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Features;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class FeatureMap : IEntityTypeConfiguration<Feature>
{
    public void Configure(EntityTypeBuilder<Feature> builder)
    {
        builder.ToTable("Features");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedUtc)
            .HasColumnName("Created")
            .HasConversion<UtcDateTimeConverter>();

        // Transition shadow for the UTC column-name standardisation (Created -> CreatedUtc).
        builder.Property<DateTime?>("CreatedUtcColumn")
            .HasColumnName("CreatedUtc")
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.Id)
            .HasColumnName("FeatureId");
    }
}
