using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Features;
using ODK.Core.Venues;

namespace ODK.Web.Razor.Models.Admin.Events;

public class EventFormViewModel : EventFormSubmitViewModel
{
    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<ChapterVenue> ChapterVenues { get; init; }

    public required Currency Currency { get; init; }

    public required IReadOnlyCollection<SiteFeatureType> OwnerSubscriptionFeatures { get; init; }
}