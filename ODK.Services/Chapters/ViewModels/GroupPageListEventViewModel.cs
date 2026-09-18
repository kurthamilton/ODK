using ODK.Core.Events;
using ODK.Core.Venues;
using ODK.Data.Core.Events;

namespace ODK.Services.Chapters.ViewModels;

public class GroupPageListEventViewModel
{
    public required ChapterVenue? ChapterVenue { get; init; }

    public required Event Event { get; init; }

    public required EventResponse? Response { get; init; }

    public required EventResponseSummaryDto? ResponseSummary { get; init; }
}
