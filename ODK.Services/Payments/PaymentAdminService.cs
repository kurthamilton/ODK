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

    public async Task<ChapterPaymentsViewModel> GetPayments(MemberChapterServiceRequest request)
    {
        var platform = request.Platform;

        var (chapter, payments) = await GetChapterAdminRestrictedContent(
            ChapterAdminSecurable.Payments,
            request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.PaymentRepository.GetMemberDtosByChapterId(request.ChapterId));

        return new ChapterPaymentsViewModel
        {
            Chapter = chapter,
            Payments = payments
                .OrderByDescending(x => x.Payment.PaidUtc)
                .ToArray(),
            Platform = platform
        };
    }
}