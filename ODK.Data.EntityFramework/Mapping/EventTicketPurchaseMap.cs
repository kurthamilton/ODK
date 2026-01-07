using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Events;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class EventTicketPurchaseMap : IEntityTypeConfiguration<EventTicketPurchase>
{
    public void Configure(EntityTypeBuilder<EventTicketPurchase> builder)
    {
        builder.ToTable("EventTicketPurchases");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DepositPurchasedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.Id)
            .HasColumnName("EventTicketPurchaseId");

        builder.Property(x => x.PurchasedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.HasOne<Event>()
            .WithMany()
            .HasForeignKey(x => x.EventId);

        builder.HasOne(x => x.Member)
            .WithMany()
            .HasForeignKey(x => x.MemberId);
    }
}
