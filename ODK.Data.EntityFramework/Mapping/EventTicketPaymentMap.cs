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

        // Stated rather than left to convention, which would cascade: the database restricts, so deleting an
        // event cannot take the record of what was paid for it.
        builder.HasOne<Event>()
            .WithMany()
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Payment)
            .WithOne()
            .HasForeignKey<EventTicketPayment>(x => x.PaymentId);
    }
}