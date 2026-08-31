using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Data.EntityFramework.Converters;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Mapping;

public class PaymentMap : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ActualAmount)
            .IsMoneyType();

        builder.Property(x => x.ActualCommissionAmount)
            .IsMoneyType();

        builder.Property(x => x.ActualConnectedAccountAmount)
            .IsMoneyType();

        builder.Property(x => x.ActualFeeAmount)
            .IsMoneyType();

        builder.Property(x => x.ActualNetAmount)
            .IsMoneyType();

        builder.Property(x => x.Amount)
            .IsMoneyType();

        builder.Property(x => x.CreatedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.Environment)
            .HasColumnName("EnvironmentTypeId")
            .HasConversion<int?>();

        builder.Property(x => x.ExternalChargeId)
            .HasMaxLength(100);

        builder.Property(x => x.ExternalTransferId)
            .HasMaxLength(100);

        builder.Property(x => x.PaidUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.PaymentProvider)
            .HasColumnName("PaymentProviderTypeId")
            .HasConversion<int?>();

        builder.Property(x => x.Platform)
            .HasColumnName("PlatformTypeId")
            .HasConversion<int?>();

        builder.Property(x => x.ReconciliationFailedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.ReconciliationFailureReason)
            .HasMaxLength(500);

        builder.Property(x => x.ReconciliationIgnoredUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.SettlementCurrencyCode)
            .HasMaxLength(3);

        builder.Property(x => x.TransferredUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.HasOne(x => x.Currency)
            .WithMany()
            .HasForeignKey(x => x.CurrencyId);

        // Cascade on both, matching the constraints the database already enforces.
        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}