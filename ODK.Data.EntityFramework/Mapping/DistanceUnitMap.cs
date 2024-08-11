using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Countries;

namespace ODK.Data.EntityFramework.Mapping;

public class DistanceUnitMap : IEntityTypeConfiguration<DistanceUnit>
{
    public void Configure(EntityTypeBuilder<DistanceUnit> builder)
    {
        builder.ToTable("DistanceUnits");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("DistanceUnitId");
    }
}
