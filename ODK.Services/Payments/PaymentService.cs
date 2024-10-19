using ODK.Core.Chapters;
using ODK.Core.Countries;
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

    public async Task<ServiceResult> ActivateSubscriptionPlan(IPaymentSettings settings, string externalId)
    {
        var provider = _paymentProviderFactory.GetPaymentProvider(settings);
        return await provider.ActivateSubscriptionPlan(externalId);
    }

    public async Task<ServiceResult> CancelSubscription(IPaymentSettings settings, string externalId)
    {
        var provider = _paymentProviderFactory.GetPaymentProvider(settings);
        var result = await provider.CancelSubscription(externalId);
        return result
            ? ServiceResult.Successful()
            : ServiceResult.Failure("Error canceling subscription");
    }

    public async Task<string?> CreateProduct(IPaymentSettings settings, string name)
    {
        var provider = _paymentProviderFactory.GetPaymentProvider(settings);
        return await provider.CreateProduct(name);
    }

    public async Task<string?> CreateSubscriptionPlan(IPaymentSettings settings, ExternalSubscriptionPlan subscriptionPlan)
    {
        var provider = _paymentProviderFactory.GetPaymentProvider(settings);
        return await provider.CreateSubscriptionPlan(subscriptionPlan);
    }

    public async Task<ServiceResult> DeactivateSubscriptionPlan(IPaymentSettings settings, string externalId)
    {
        var provider = _paymentProviderFactory.GetPaymentProvider(settings);
        return await provider.DeactivateSubscriptionPlan(externalId);
    }

    public async Task<string?> GetProductId(IPaymentSettings settings, string name)
    {
        var provider = _paymentProviderFactory.GetPaymentProvider(settings);
        return await provider.GetProductId(name);
    }

    public async Task<ExternalSubscription?> GetSubscription(IPaymentSettings settings, string externalId)
    {
        var provider = _paymentProviderFactory.GetPaymentProvider(settings);
        return await provider.GetSubscription(externalId);
    }

    public async Task<ExternalSubscriptionPlan?> GetSubscriptionPlan(IPaymentSettings settings, string externalId)
    {
        var provider = _paymentProviderFactory.GetPaymentProvider(settings);
        return await provider.GetSubscriptionPlan(externalId);
    }

    public async Task<ServiceResult> MakePayment(ChapterPaymentSettings chapterPaymentSettings, 
        Currency currency, Member member, decimal amount, string cardToken, string reference)
    {
        var sitePaymentSettings = await _unitOfWork.SitePaymentSettingsRepository.GetActive().Run();

        var sitePaymentProvider = _paymentProviderFactory.GetPaymentProvider(sitePaymentSettings);

        var paymentProvider = chapterPaymentSettings.HasApiKey
            ? _paymentProviderFactory.GetPaymentProvider(chapterPaymentSettings)
            : sitePaymentProvider;

        var paymentResult = await paymentProvider.MakePayment(currency.Code, amount, cardToken, reference,
            member.FullName);
        if (!paymentResult.Success)
        {
            return paymentResult;
        }

        var payment = new Payment
        {
            Amount = amount,
            ChapterId = chapterPaymentSettings.ChapterId,
            CurrencyId = currency.Id,
            MemberId = member.Id,
            PaidUtc = DateTime.UtcNow,
            Reference = reference
        };
        _unitOfWork.PaymentRepository.Add(payment);
        await _unitOfWork.SaveChangesAsync();

        if (!chapterPaymentSettings.HasApiKey && !string.IsNullOrEmpty(chapterPaymentSettings.EmailAddress))
        {
            amount = Math.Round(amount - (0.025M * amount), 2);
            await sitePaymentProvider.SendPayment(currency.Code, amount,
                chapterPaymentSettings.EmailAddress, payment.Id.ToString(), reference);
        }        

        return ServiceResult.Successful();
    }
}
