using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Payments;

public class PaymentCommissionTextViewModel
{
    public required ChapterPaymentSettings? ChapterPaymentSettings { get; init; }

    public required MemberSiteSubscription? OwnerSubscription { get; init; }
}
