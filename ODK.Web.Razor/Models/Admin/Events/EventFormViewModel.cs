using ODK.Core.Chapters;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Venues;

namespace ODK.Web.Razor.Models.Admin.Events;

public class EventFormViewModel : EventFormSubmitViewModel
{
    public required Chapter Chapter { get; init; }

    public required MemberSiteSubscription? OwnerSubscription { get; init; }

    public required ChapterPaymentSettings PaymentSettings { get; init; }

    public required PlatformType Platform { get; init; }

    public bool TicketsEnabled => OwnerSubscription?.HasFeature(SiteFeatureType.EventTickets) == true;

    public required IReadOnlyCollection<Venue> Venues { get; init; }
}
