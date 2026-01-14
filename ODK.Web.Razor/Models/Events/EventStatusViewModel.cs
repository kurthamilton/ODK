using ODK.Core.Chapters;
using ODK.Core.Events;

namespace ODK.Web.Razor.Models.Events;

public class EventStatusViewModel
{
    public required Chapter Chapter { get; init; }

    public required Event Event { get; init; }

    public required int? SpacesLeft { get; init; }
}