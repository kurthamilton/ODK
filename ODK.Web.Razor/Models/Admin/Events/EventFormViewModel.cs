using ODK.Core.Members;
using ODK.Core.Venues;

namespace ODK.Web.Razor.Models.Admin.Events;

public class EventFormViewModel : EventFormSubmitViewModel
{
    public required MemberSiteSubscription? OwnerSubscription { get; init; }

    public required IReadOnlyCollection<Venue> Venues { get; init; }
}
