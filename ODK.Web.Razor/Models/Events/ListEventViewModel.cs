using ODK.Services.Events;

namespace ODK.Web.Razor.Models.Events;

public class ListEventViewModel
{
    public required string ChapterName { get; init; }

    public required EventResponseViewModel Event { get; init; }
}
