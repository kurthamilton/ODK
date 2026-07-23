using ODK.Core.Events;

namespace ODK.Data.Core.Events;

public class EventWithLocalDateDto
{
    public required DateTime DateLocal { get; init; }

    public required Event Event { get; init; }
}
