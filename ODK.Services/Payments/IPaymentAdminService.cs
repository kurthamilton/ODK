using ODK.Services.Payments.Models;
using ODK.Services.Payments.ViewModels;

namespace ODK.Services.Payments;

public interface IPaymentAdminService
{
    Task CreateReconciliation(MemberChapterServiceRequest request, CreateReconciliationModel model);

    Task<ChapterPaymentsViewModel> GetPayments(MemberChapterServiceRequest request);

    Task<ChapterReconciliationsViewModel> GetReconciliations(MemberChapterServiceRequest request);

    Task<IReadOnlyCollection<MissingPaymentModel>> GetMissingPayments(Guid currentMemberId);

    Task SetPaymentReconciliationExemption(MemberChapterServiceRequest request, Guid paymentId, bool exempt);
}
