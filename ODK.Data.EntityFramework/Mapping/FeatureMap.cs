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
            .HasConversion<UtcDateTimeConverter>();

        // Pinned to the current column name - the columns are renamed to match in a later migration.
        builder.Property(x => x.DescriptionHtml)
            .HasColumnName("Description")
            .HasMaxLength(255);

        builder.Property(x => x.Name)
            .HasMaxLength(255);
    }
}
