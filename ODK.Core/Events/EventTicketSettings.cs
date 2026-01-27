using ODK.Core.Countries;

namespace ODK.Core.Events;

public class EventTicketSettings : ICloneable<EventTicketSettings>
{
    public decimal Cost { get; set; }

    public Currency Currency { get; set; } = null!;

    public Guid CurrencyId { get; set; }

    public decimal? Deposit { get; set; }

    public Guid EventId { get; set; }

    public EventTicketSettings Clone() => new()
    {
        Cost = Cost,
        Currency = Currency,
        CurrencyId = CurrencyId,
        Deposit = Deposit,
        EventId = EventId
    };
}