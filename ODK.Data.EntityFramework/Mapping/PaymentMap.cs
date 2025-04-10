﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Countries;
using ODK.Core.Payments;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class PaymentMap : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("PaymentId");

        builder.Property(x => x.PaidUtc)
            .HasColumnName("PaidDate")
            .HasConversion<UtcDateTimeConverter>();        

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(x => x.CurrencyId);

        builder.HasOne<PaymentReconciliation>()
            .WithMany()
            .HasForeignKey(x => x.PaymentReconciliationId);
    }
}
