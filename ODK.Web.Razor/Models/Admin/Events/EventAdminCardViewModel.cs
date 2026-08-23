using ODK.Core.Chapters;
using ODK.Data.Core.Events;

namespace ODK.Web.Razor.Models.Admin.Events;

public class EventAdminCardViewModel
{
    public required Chapter Chapter { get; init; }

    public required EventSummaryDto Summary { get; init; }
}
