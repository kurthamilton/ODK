namespace ODK.Core.Events;

public class EventTicketSettings
{
    public decimal Cost { get; set; }

    public decimal? Deposit { get; set; }

    public Guid EventId { get; set; }
}
