using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Events;

namespace ODK.Data.EntityFramework.Mapping;

public class EventTicketSettingsMap : IEntityTypeConfiguration<EventTicketSettings>
{
    public void Configure(EntityTypeBuilder<EventTicketSettings> builder)
    {
        builder.ToTable("EventTicketSettings");

        builder.HasKey(x => x.EventId);

        builder.HasOne(x => x.Currency)
            .WithMany()
            .HasForeignKey(x => x.CurrencyId);
    }
}