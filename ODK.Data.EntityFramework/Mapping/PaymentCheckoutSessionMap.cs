using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class PaymentCheckoutSessionMap : IEntityTypeConfiguration<PaymentCheckoutSession>
{
    public void Configure(EntityTypeBuilder<PaymentCheckoutSession> builder)
    {
        builder.ToTable("PaymentCheckoutSessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CompletedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.ExpiredUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.Id)
            .HasColumnName("PaymentCheckoutSessionId");

        builder.Property(x => x.StartedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}