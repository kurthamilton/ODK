using ODK.Data.Core;
using ODK.Services.Payments.ViewModels;

namespace ODK.Services.Payments;

public class PaymentAdminService : OdkAdminServiceBase, IPaymentAdminService
{
    public PaymentAdminService(IUnitOfWork unitOfWork)
        : base(unitOfWork)
    {
    }

    public async Task<ChapterPaymentsViewModel> GetPayments(
        IMemberChapterAdminServiceRequest request)
    {
        var (environment, chapter) = (request.Environment, request.Chapter);

        var (payments, paymentAccount) = await GetChapterAdminRestrictedContent(
            request,
            x => x.PaymentRepository.GetMemberDtosByChapterId(chapter.Id),
            x => x.ChapterPaymentAccountRepository
                .Query()
                .ForChapter(chapter.Id)
                .ForEnvironment(environment)
                .GetSingleOrDefault());

        return new ChapterPaymentsViewModel
        {
            Chapter = chapter,
            PaymentAccountEnabled = paymentAccount?.SetupComplete() == true,
            Payments = payments
                .OrderByDescending(x => x.Payment.PaidUtc)
                .ToArray()
        };
    }
}
