using ODK.Core.Features;
using ODK.Core.Subscriptions;

namespace ODK.Web.Razor.Models.Admin.Events;

public class EventSettingsFormViewModel : EventSettingsFormSubmitViewModel
{
    public required SiteSubscription? OwnerSubscription { get; init; }

    public bool ScheduledEmailsEnabled => OwnerSubscription?.HasFeature(SiteFeatureType.ScheduledEventEmails) == true;
}
