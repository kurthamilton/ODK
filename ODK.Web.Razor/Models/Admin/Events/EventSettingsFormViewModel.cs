using ODK.Core.Features;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Admin.Events;

public class EventSettingsFormViewModel : EventSettingsFormSubmitViewModel
{
    public required MemberSiteSubscription? OwnerSubscription { get; init; }

    public bool ScheduledEmailsEnabled => OwnerSubscription?.HasFeature(SiteFeatureType.ScheduledEventEmails) == true;
}
