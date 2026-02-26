using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Services.Events.ViewModels;

namespace ODK.Web.Razor.Models.Events;

public class EventListViewModel
{
    public required Chapter Chapter { get; init; }

    public string? Class { get; init; }

    public required IReadOnlyCollection<EventResponseViewModel> Events { get; init; }

    public bool FullWidth { get; init; }

    public bool ForceDisplayYear { get; init; }

    public required PlatformType Platform { get; init; }

    public required TimeZoneInfo TimeZone { get; init; }
}
