using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Venues;

namespace ODK.Web.Razor.Models.Events;

public class EventHeaderViewModel
{
    public required Chapter Chapter { get; init; }

    public required Event Event { get; init; }

    public required Venue? Venue { get; init; }
}