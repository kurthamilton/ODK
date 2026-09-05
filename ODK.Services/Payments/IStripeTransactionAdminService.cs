using ODK.Services.Payments.ViewModels;

namespace ODK.Services.Payments;

public interface IStripeTransactionAdminService
{
    Task<SiteAdminStripeTransactionsViewModel> GetStripeTransactionsViewModel(IMemberServiceRequest request);
}
