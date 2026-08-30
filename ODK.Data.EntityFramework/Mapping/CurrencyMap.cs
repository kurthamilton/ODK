using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Countries;

namespace ODK.Data.EntityFramework.Mapping;

public class CurrencyMap : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("Currencies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(255);

        builder.Property(x => x.CountryIsoCode2)
            .HasMaxLength(2);

        builder.Property(x => x.CountryIsoCode3)
            .HasMaxLength(3);

        builder.Property(x => x.CountryName)
            .HasMaxLength(255);

        builder.Property(x => x.Name)
            .HasMaxLength(255);

        builder.Property(x => x.Symbol)
            .HasMaxLength(5);
    }
}
