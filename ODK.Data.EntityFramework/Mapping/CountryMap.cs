using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Countries;

namespace ODK.Data.EntityFramework.Mapping;

public class CountryMap : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("CountryId");

        builder.Property(x => x.IsoCode2)
            .HasMaxLength(2);

        builder.Property(x => x.IsoCode3)
            .HasMaxLength(3);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(x => x.CurrencyId);
    }
}
