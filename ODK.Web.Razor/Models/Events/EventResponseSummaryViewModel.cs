using ODK.Core.Events;
using ODK.Data.Core.Events;

namespace ODK.Web.Razor.Models.Events;

public class EventResponseSummaryViewModel
{
    public string? Class { get; init; }

    public required EventResponseType? MemberResponse { get; init; }

    public required EventResponseSummaryDto? Summary { get; init; }
}
