using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Payments;

namespace ODK.Data.EntityFramework.Mapping;

public class SitePaymentSettingsMap : IEntityTypeConfiguration<SitePaymentSettings>
{
    public void Configure(EntityTypeBuilder<SitePaymentSettings> builder)
    {
        builder.ToTable("SitePaymentSettings");

        builder.HasKey(x => x.Id);
    }
}
