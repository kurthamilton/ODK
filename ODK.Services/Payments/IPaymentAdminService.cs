using ODK.Services.Payments.ViewModels;

namespace ODK.Services.Payments;

public interface IPaymentAdminService
{
    Task<ChapterPaymentsViewModel> GetPayments(MemberChapterServiceRequest request);
}