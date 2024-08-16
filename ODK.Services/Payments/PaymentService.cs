using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Data.Core;

namespace ODK.Services.Payments;

public class PaymentService : IPaymentService
{
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(IUnitOfWork unitOfWork, IPaymentProviderFactory paymentProviderFactory)
    {
        _paymentProviderFactory = paymentProviderFactory;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> MakePayment(Guid chapterId, Member member, decimal amount, string cardToken, string reference)
    {
        var (settings, chapter) = await _unitOfWork.RunAsync(
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterId),
            x => x.ChapterRepository.GetById(chapterId));

        if (settings == null || settings.Provider == null)
        {
            return ServiceResult.Failure("Payment settings not found");
        }

        var providerType = Enum.Parse<PaymentProviderType>(settings.Provider, true);
        
        var paymentProvider = _paymentProviderFactory.GetPaymentProvider(providerType);

        var paymentResult = await paymentProvider.MakePayment(settings, settings.Currency.Code, amount, cardToken, reference,
            member.FullName);
        if (!paymentResult.Success)
        {
            return paymentResult;
        }

        _unitOfWork.PaymentRepository.Add(new Payment
        {
            Amount = (double)amount,
            CurrencyCode = settings.Currency.Code,
            MemberId = member.Id,
            PaidUtc = DateTime.UtcNow,
            Reference = reference
        });
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }
}
