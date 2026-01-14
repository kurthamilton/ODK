using ODK.Core.Countries;

namespace ODK.Core.Events;

public class EventTicketSettings
{
    public decimal Cost { get; set; }

    public Currency Currency { get; set; } = null!;

    public Guid CurrencyId { get; set; }

    public decimal? Deposit { get; set; }

    public Guid EventId { get; set; }
}