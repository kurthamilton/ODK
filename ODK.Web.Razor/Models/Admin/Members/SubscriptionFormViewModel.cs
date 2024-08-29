using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Admin.Members;

public class SubscriptionFormViewModel : SubscriptionFormSubmitViewModel
{
    public required ChapterPaymentSettings? PaymentSettings { get; init; }
}
