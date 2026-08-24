using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Payments;

namespace ODK.Data.EntityFramework.Mapping;

public class SitePaymentProductMap : IEntityTypeConfiguration<SitePaymentProduct>
{
    public void Configure(EntityTypeBuilder<SitePaymentProduct> builder)
    {
        builder.ToTable("SitePaymentProducts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Platform)
            .HasColumnName("PlatformTypeId")
            .HasConversion<int>();

        /* A platform owns one product per payment settings account: a second would make "the product to
           create this price under" ambiguous. Platform leads so the same index answers the lookup by
           platform alone. */
        builder.HasIndex(x => new { x.Platform, x.SitePaymentSettingId })
            .IsUnique();

        builder.HasOne<SitePaymentSettings>()
            .WithMany()
            .HasForeignKey(x => x.SitePaymentSettingId);
    }
}
