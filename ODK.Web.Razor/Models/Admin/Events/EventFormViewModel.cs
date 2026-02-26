using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Features;
using ODK.Core.Subscriptions;
using ODK.Core.Venues;

namespace ODK.Web.Razor.Models.Admin.Events;

public class EventFormViewModel : EventFormSubmitViewModel
{
    public required Chapter Chapter { get; init; }

    public required Currency Currency { get; init; }

    public required SiteSubscription? OwnerSubscription { get; init; }

    public bool TicketsEnabled => OwnerSubscription?.HasFeature(SiteFeatureType.EventTickets) == true;

    public required IReadOnlyCollection<Venue> Venues { get; init; }
}