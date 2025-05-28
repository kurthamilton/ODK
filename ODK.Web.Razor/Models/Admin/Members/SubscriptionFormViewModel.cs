using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Admin.Members;

public class SubscriptionFormViewModel : SubscriptionFormSubmitViewModel
{
    public Guid? Id { get; init; }

    public required MemberSiteSubscription? OwnerSubscription { get; init; }

    public required ChapterPaymentSettings PaymentSettings { get; init; }

    public required bool SupportsRecurringPayments { get; init; }
}
