using ODK.Core;
using ODK.Core.Subscriptions;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Services.Payments;
using ODK.Services.Subscriptions.ViewModels;

namespace ODK.Services.Subscriptions;

public class SiteSubscriptionAdminService : OdkAdminServiceBase, ISiteSubscriptionAdminService
{
    private readonly IHtmlSanitizer _htmlSanitizer;
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly IUnitOfWork _unitOfWork;

    public SiteSubscriptionAdminService(
        IUnitOfWork unitOfWork,
        IHtmlSanitizer htmlSanitizer,
        IPaymentProviderFactory paymentProviderFactory) 
        : base(unitOfWork)
    {
        _htmlSanitizer = htmlSanitizer;
        _paymentProviderFactory = paymentProviderFactory;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> AddSiteSubscription(
        MemberServiceRequest request, SiteSubscriptionCreateModel model)
    {
        var (currentMemberId, platform) = (request.CurrentMemberId, request.Platform);

        var (paymentSettings, existing) = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.SitePaymentSettingsRepository.GetById(model.SitePaymentSettingId),
            x => x.SiteSubscriptionRepository.GetAll(platform));

        if (existing.Any(x => 
            x.Platform == platform && 
            x.SitePaymentSettingId == model.SitePaymentSettingId &&
            string.Equals(x.Name, model.Name, StringComparison.InvariantCultureIgnoreCase)))
        {
            return ServiceResult.Failure($"Subscription '{model.Name}' already exists");
        }

        if (model.FallbackSiteSubscriptionId != null && 
            existing.All(x => x.Id != model.FallbackSiteSubscriptionId))
        {
            return ServiceResult.Failure($"Fallback subscription not found");
        }

        var subscription = new SiteSubscription
        {
            Platform = platform,
            SitePaymentSettingId = paymentSettings.Id
        };

        UpdateSiteSubscription(model, subscription);

        var paymentProvider = _paymentProviderFactory.GetSitePaymentProvider(paymentSettings);

        subscription.ExternalProductId = await paymentProvider.CreateProduct(subscription.Name);

        _unitOfWork.SiteSubscriptionRepository.Add(subscription);

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> AddSiteSubscriptionPrice(
        MemberServiceRequest request, 
        Guid siteSubscriptionId,
        SiteSubscriptionPriceCreateModel model)
    {
        var currentMemberId = request.CurrentMemberId;

        var (siteSubscription, existing, currency) = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.SiteSubscriptionRepository.GetById(siteSubscriptionId),
            x => x.SiteSubscriptionPriceRepository.GetBySiteSubscriptionId(siteSubscriptionId),
            x => x.CurrencyRepository.GetById(model.CurrencyId));

        if (existing.Any(x => x.CurrencyId == model.CurrencyId && x.Frequency == model.Frequency))
        {
            return ServiceResult.Failure($"Subscription already has a price for currency '{currency.Code}'");
        }

        if (model.Frequency == SiteSubscriptionFrequency.None || !Enum.IsDefined(model.Frequency))
        {
            return ServiceResult.Failure("Invalid frequency");
        }

        var price = new SiteSubscriptionPrice
        {
            Amount = model.Amount,
            CurrencyId = model.CurrencyId,
            Frequency = model.Frequency,
            SiteSubscriptionId = siteSubscriptionId
        };

        var paymentProvider = _paymentProviderFactory.GetSitePaymentProvider(siteSubscription.SitePaymentSettings);

        if (!string.IsNullOrEmpty(siteSubscription.ExternalProductId) && model.Amount > 0)
        {
            price.ExternalId = await paymentProvider.CreateSubscriptionPlan(
                new ExternalSubscriptionPlan
                {
                    Amount = model.Amount,
                    CurrencyCode = currency.Code,
                    ExternalId = "",
                    ExternalProductId = siteSubscription.ExternalProductId,
                    Frequency = model.Frequency,
                    Name = $"{siteSubscription.Name} - {model.Frequency} [{currency.Code}]",
                    NumberOfMonths = model.Frequency == SiteSubscriptionFrequency.Yearly ? 12 : 1,
                    Recurring = true
                });
        }

        _unitOfWork.SiteSubscriptionPriceRepository.Add(price);
        await _unitOfWork.SaveChangesAsync();

        if (!string.IsNullOrEmpty(price.ExternalId))
        {
            await paymentProvider.ActivateSubscriptionPlan(price.ExternalId);
        }

        return ServiceResult.Successful();
    }

    public async Task DeleteSiteSubscriptionPrice(Guid currentMemberId, Guid siteSubscriptionId, Guid siteSubscriptionPriceId)
    {
        var (siteSubscription, price) = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.SiteSubscriptionRepository.GetById(siteSubscriptionId),
            x => x.SiteSubscriptionPriceRepository.GetById(siteSubscriptionPriceId));

        OdkAssertions.MeetsCondition(price, x => x.SiteSubscriptionId == siteSubscriptionId);

        _unitOfWork.SiteSubscriptionPriceRepository.Delete(price);
        await _unitOfWork.SaveChangesAsync();

        if (!string.IsNullOrEmpty(price.ExternalId))
        {
            var paymentProvider = _paymentProviderFactory.GetSitePaymentProvider(
                siteSubscription.SitePaymentSettings);
            await paymentProvider.DeactivateSubscriptionPlan(price.ExternalId);
        }
    }

    public async Task<IReadOnlyCollection<SiteSubscription>> GetAllSubscriptions(
        MemberServiceRequest request)
    {
        var (currentMemberId, platform) = (request.CurrentMemberId, request.Platform);

        return await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.SiteSubscriptionRepository.GetAll(platform));
    }

    public async Task<IReadOnlyCollection<SiteSubscriptionSuperAdminListItemViewModel>> GetSiteSubscriptionSuperAdminListItems(
        MemberServiceRequest request)
    {
        var platform = request.Platform;

        var (siteSubscriptions, prices) = await GetSuperAdminRestrictedContent(request.CurrentMemberId,
            x => x.SiteSubscriptionRepository.GetAll(platform),
            x => x.SiteSubscriptionPriceRepository.GetAll(platform));

        var priceCounts = prices
            .GroupBy(x => x.SiteSubscriptionId)
            .ToDictionary(x => x.Key, x => x.Count());

        return siteSubscriptions
            .Select(x => new SiteSubscriptionSuperAdminListItemViewModel
            {
                Default = x.Default,
                Enabled = x.Enabled,
                GroupLimit = x.GroupLimit,
                Id = x.Id,
                MemberLimit = x.MemberLimit,
                MemberSubscriptions = x.MemberSubscriptions,                
                Name = x.Name,
                PaymentSettingsName = x.SitePaymentSettings.Name,
                Premium = x.Premium,
                PriceCount = priceCounts.TryGetValue(x.Id, out var priceCount) ? priceCount : 0,
                SendMemberEmails = x.SendMemberEmails
            })
            .OrderBy(x => x.PaymentSettingsName)
            .ThenBy(x => x.Name)
            .ToArray();
    }

    public async Task<SiteSubscriptionViewModel> GetSubscriptionViewModel(Guid currentMemberId, Guid siteSubscriptionId)
    {
        var (subscription, prices, currencies, sitePaymentSettings) = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.SiteSubscriptionRepository.GetById(siteSubscriptionId),
            x => x.SiteSubscriptionPriceRepository.GetBySiteSubscriptionId(siteSubscriptionId),
            x => x.CurrencyRepository.GetAll(),
            x => x.SitePaymentSettingsRepository.GetAll());

        return new SiteSubscriptionViewModel
        {
            Currencies = currencies,
            Prices = prices,
            SitePaymentSettings = sitePaymentSettings,
            Subscription = subscription
        };
    }

    public async Task MakeDefault(MemberServiceRequest request, Guid siteSubscriptionId)
    {
        var (currentMemberId, platform) = (request.CurrentMemberId, request.Platform);

        var subscriptions = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.SiteSubscriptionRepository.GetAll(platform));

        var subscription = subscriptions.FirstOrDefault(x => x.Id == siteSubscriptionId);
        OdkAssertions.Exists(subscription);

        var existingDefaults = subscriptions
            .Where(x => x.Default && x.SitePaymentSettingId == subscription.SitePaymentSettingId)
            .ToArray();

        foreach (var existingDefault in existingDefaults)
        {
            existingDefault.Default = false;
            _unitOfWork.SiteSubscriptionRepository.Update(existingDefault);
        }

        subscription.Default = true;
        _unitOfWork.SiteSubscriptionRepository.Update(subscription);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ServiceResult> UpdateSiteSubscription(
        MemberServiceRequest request, Guid siteSubscriptionId, SiteSubscriptionCreateModel model)
    {
        var (currentMemberId, platform) = (request.CurrentMemberId, request.Platform);

        var existing = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.SiteSubscriptionRepository.GetAll(platform));

        var subscription = existing.FirstOrDefault(x => x.Id == siteSubscriptionId);
        OdkAssertions.Exists(subscription);

        if (model.FallbackSiteSubscriptionId != subscription.FallbackSiteSubscriptionId && 
            model.FallbackSiteSubscriptionId != null)
        {
            var fallback = existing.FirstOrDefault(x => x.Id == model.FallbackSiteSubscriptionId);
            if (fallback == null)
            {
                return ServiceResult.Failure("Fallback subscription not found");
            }

            if (fallback.Id == subscription.Id)
            {
                return ServiceResult.Failure("Subscription cannot fallback to itself");
            }
        }

        UpdateSiteSubscription(model, subscription);

        _unitOfWork.SiteSubscriptionRepository.Update(subscription);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateSiteSubscriptionEnabled(Guid currentMemberId, Guid siteSubscriptionId,
        bool enabled)
    {
        var subscription = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.SiteSubscriptionRepository.GetById(siteSubscriptionId));

        subscription.Enabled = enabled;

        _unitOfWork.SiteSubscriptionRepository.Update(subscription);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    private void UpdateSiteSubscription(SiteSubscriptionCreateModel model, SiteSubscription subscription)
    {
        subscription.Description = _htmlSanitizer.Sanitize(model.Description, DefaultHtmlSantizerOptions);
        subscription.Enabled = model.Enabled;
        subscription.FallbackSiteSubscriptionId = model.FallbackSiteSubscriptionId;
        subscription.GroupLimit = model.GroupLimit;
        subscription.MemberLimit = model.MemberLimit;
        subscription.MemberSubscriptions = model.MemberSubscriptions;
        subscription.Name = model.Name;
        subscription.Premium = model.Premium;
        subscription.SendMemberEmails = model.SendMemberEmails;                
    }
}
