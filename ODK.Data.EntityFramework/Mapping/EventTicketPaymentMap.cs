using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Events;

namespace ODK.Data.EntityFramework.Mapping;

public class EventTicketPaymentMap : IEntityTypeConfiguration<EventTicketPayment>
{
    public void Configure(EntityTypeBuilder<EventTicketPayment> builder)
    {
        builder.ToTable("EventTicketPayments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("EventTicketPaymentId");

        builder.HasOne<Event>()
            .WithMany()
            .HasForeignKey(x => x.EventId);

        builder.HasOne(x => x.Payment)
            .WithOne()
            .HasForeignKey<EventTicketPayment>(x => x.PaymentId);
    }
}