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

    public async Task<ServiceResult> MakePayment(Member member, double amount, string cardToken, string reference)
    {
        var (paymentSettings, chapter, country) = await _unitOfWork.RunAsync(
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(member.ChapterId),
            x => x.ChapterRepository.GetById(member.ChapterId),
            x => x.CountryRepository.GetByChapterId(member.ChapterId));

        if (paymentSettings == null || paymentSettings.Provider == null)
        {
            return ServiceResult.Failure("Payment settings not found");
        }

        var providerType = Enum.Parse<PaymentProviderType>(paymentSettings.Provider, true);
        
        var paymentProvider = _paymentProviderFactory.GetPaymentProvider(providerType);

        var paymentResult = await paymentProvider.MakePayment(paymentSettings, country.CurrencyCode, amount, cardToken, reference,
            member.FullName);
        if (!paymentResult.Success)
        {
            return paymentResult;
        }

        _unitOfWork.PaymentRepository.Add(new Payment
        {
            Amount = amount,
            CurrencyCode = country.CurrencyCode,
            MemberId = member.Id,
            PaidUtc = DateTime.UtcNow,
            Reference = reference
        });
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }
}
