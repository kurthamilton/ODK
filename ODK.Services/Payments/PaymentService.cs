using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Data.Core;
using ODK.Services.Logging;

namespace ODK.Services.Payments;

public class PaymentService : IPaymentService
{
    private readonly ILoggingService _loggingService;
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(
        IUnitOfWork unitOfWork, 
        IPaymentProviderFactory paymentProviderFactory,
        ILoggingService loggingService)
    {
        _loggingService = loggingService;
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

    public async Task<ExternalCheckoutSession?> GetCheckoutSession(IPaymentSettings settings, string externalId)
    {
        var provider = _paymentProviderFactory.GetPaymentProvider(settings);
        return await provider.GetCheckoutSession(externalId);
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

    public async Task<ServiceResult> MakePayment(
        ChapterPaymentSettings chapterPaymentSettings, 
        Currency currency, 
        Member member, 
        decimal amount, 
        string cardToken, 
        string reference)
    {
        var sitePaymentSettings = await _unitOfWork.SitePaymentSettingsRepository.GetActive().Run();

        var sitePaymentProvider = _paymentProviderFactory.GetPaymentProvider(sitePaymentSettings);

        var paymentProvider = chapterPaymentSettings.HasApiKey
            ? _paymentProviderFactory.GetPaymentProvider(chapterPaymentSettings)
            : sitePaymentProvider;

        var paymentResult = await paymentProvider.MakePayment(
            currency.Code, 
            amount, 
            cardToken, 
            reference,
            member.Id,
            member.FullName);
        if (!paymentResult.Success)
        {
            await _loggingService.Error(
                $"Error making payment for member {member.Id} for {amount} with reference '{reference}': {paymentResult.Message}");
            return ServiceResult.Failure(paymentResult.Message);
        }

        var payment = new Payment
        {
            Amount = amount,
            ChapterId = chapterPaymentSettings.ChapterId,
            CurrencyId = currency.Id,
            ExternalId = paymentResult.Id,
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

    public async Task<ExternalCheckoutSession> StartCheckoutSession(
        IPaymentSettings settings, 
        ExternalSubscriptionPlan subscriptionPlan,
        string returnPath)
    {
        var provider = _paymentProviderFactory.GetPaymentProvider(settings);
        return await provider.StartCheckout(subscriptionPlan, returnPath);
    }
}
