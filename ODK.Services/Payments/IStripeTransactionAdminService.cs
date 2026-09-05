using ODK.Services.Payments.ViewModels;

namespace ODK.Services.Payments;

public interface IStripeTransactionAdminService
{
    /// <summary>
    /// Writes down a subscription renewal Stripe took and nothing here recorded, as the invoice that billed
    /// it says it happened, then reads back what it settled for.
    /// </summary>
    /// <remarks>
    /// The payment and nothing else. A renewal missing its payment still extended the member's access - a
    /// subscription record was written for it against the purchase's payment - so writing another would
    /// extend the expiry a second time.
    /// </remarks>
    Task<ServiceResult> BackfillRenewalPayment(IMemberServiceRequest request, string invoiceId);

    Task<SiteAdminStripeTransactionsViewModel> GetStripeTransactionsViewModel(IMemberServiceRequest request);
}
