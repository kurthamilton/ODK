using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberPaymentSettingsMap : IEntityTypeConfiguration<MemberPaymentSettings>
{
    public void Configure(EntityTypeBuilder<MemberPaymentSettings> builder)
    {
        builder.ToTable("MemberPaymentSettings");

        builder.HasKey(x => x.MemberId);

        builder.Property(x => x.Provider)
            .HasConversion<NullableEnumStringConverter<PaymentProviderType>>();

        builder.HasOne(x => x.Currency)
            .WithMany()
            .HasForeignKey(x => x.CurrencyId);

        builder.HasOne<Member>()
            .WithOne()
            .HasForeignKey<MemberPaymentSettings>(x => x.MemberId);
    }
}
