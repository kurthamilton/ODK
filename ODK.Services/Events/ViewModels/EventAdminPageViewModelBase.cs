using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Venues;

namespace ODK.Services.Events.ViewModels;

public abstract class EventAdminPageViewModelBase
{
    public required Chapter Chapter { get; init; }

    public required Member CurrentMember { get; init; }

    public required Event Event { get; init; }

    public required PlatformType Platform { get; init; }

    public required Venue Venue { get; init; }
}
