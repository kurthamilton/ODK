using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Subscriptions;
using ODK.Core.Venues;

namespace ODK.Services.Events.ViewModels;

public abstract class EventAdminPageViewModelBase
{
    public required Chapter Chapter { get; init; }

    public required Event Event { get; init; }

    public required SiteSubscription? OwnerSubscription { get; init; }

    public required Venue Venue { get; init; }
}