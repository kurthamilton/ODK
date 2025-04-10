using ODK.Services.Payments.Models;
using ODK.Services.Payments.ViewModels;

namespace ODK.Services.Payments;

public interface IPaymentAdminService
{
    Task CreateReconciliation(AdminServiceRequest request, CreateReconciliationModel model);

    Task<ChapterPaymentsViewModel> GetPayments(AdminServiceRequest request);

    Task<ChapterReconciliationsViewModel> GetReconciliations(AdminServiceRequest request);

    Task SetPaymentReconciliationExemption(AdminServiceRequest request, Guid paymentId, bool exempt);
}
