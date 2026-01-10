using ODK.Core.Events;
using ODK.Data.Core.Events;

namespace ODK.Web.Razor.Models.Events;

public class EventResponseSummaryViewModel
{
    public required EventResponseType? MemberResponse { get; init; }

    public required EventResponseSummaryDto? Summary { get; init; }
}
