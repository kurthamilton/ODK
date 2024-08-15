using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;

namespace ODK.Services.Events.ViewModels;

public class EventsPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required Member? CurrentMember { get; init; }

    public required IReadOnlyCollection<EventResponseViewModel> Events { get; init; }

    public required PlatformType Platform { get; init; }
}
