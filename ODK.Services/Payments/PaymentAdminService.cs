using ODK.Data.Core;
using ODK.Services.Payments.ViewModels;
using ODK.Services.Security;

namespace ODK.Services.Payments;

public class PaymentAdminService : OdkAdminServiceBase, IPaymentAdminService
{
    public PaymentAdminService(
        IUnitOfWork unitOfWork,
        IPaymentProviderFactory paymentProviderFactory)
        : base(unitOfWork)
    {
    }

    public async Task<ChapterPaymentsViewModel> GetPayments(
        MemberChapterAdminServiceRequest request)
    {
        var chapter = request.Chapter;

        var payments = await GetChapterAdminRestrictedContent(
            request,
            x => x.PaymentRepository.GetMemberDtosByChapterId(chapter.Id));

        return new ChapterPaymentsViewModel
        {
            Chapter = chapter,
            Payments = payments
                .OrderByDescending(x => x.Payment.PaidUtc)
                .ToArray()
        };
    }
}