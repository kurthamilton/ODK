using ODK.Core;
using ODK.Core.Subscriptions;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Services.Payments;
using ODK.Services.Subscriptions.Models;
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

    public async Task<ServiceResult<Guid>> AddSiteSubscription(
        IMemberServiceRequest request, SiteSubscriptionCreateModel model)
    {
        var platform = request.Platform;

        var (paymentSettings, existing) = await GetSiteAdminRestrictedContent(request,
            x => x.SitePaymentSettingsRepository.GetById(model.SitePaymentSettingId),
            x => x.SiteSubscriptionRepository.GetAll(platform));

        if (existing.Any(x =>
            x.Platform == platform &&
            x.SitePaymentSettingId == model.SitePaymentSettingId &&
            string.Equals(x.Name, model.Name, StringComparison.InvariantCultureIgnoreCase)))
        {
            return ServiceResult<Guid>.Failure($"Subscription '{model.Name}' already exists");
        }

        if (model.FallbackSiteSubscriptionId != null &&
            existing.All(x => x.Id != model.FallbackSiteSubscriptionId))
        {
            return ServiceResult<Guid>.Failure($"Fallback subscription not found");
        }

        var subscription = new SiteSubscription
        {
            Id = Guid.NewGuid(),
            Platform = platform,
            SitePaymentSettingId = paymentSettings.Id
        };

        UpdateSiteSubscription(model, subscription);

        var paymentProvider = _paymentProviderFactory.GetSitePaymentProvider(paymentSettings);

        subscription.ExternalProductId = await paymentProvider.CreateProduct(subscription.Name);

        _unitOfWork.SiteSubscriptionRepository.Add(subscription);

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<Guid>.Successful(subscription.Id);
    }

    public async Task<ServiceResult> AddSiteSubscriptionPrice(
        IMemberServiceRequest request,
        Guid siteSubscriptionId,
        SiteSubscriptionPriceCreateModel model)
    {
        var (sitePaymentSettings, siteSubscription, existing, currency) = await GetSiteAdminRestrictedContent(request,
            x => x.SitePaymentSettingsRepository.GetAll(),
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

        var paymentProvider = _paymentProviderFactory.GetSitePaymentProvider(
            sitePaymentSettings,
            siteSubscription.SitePaymentSettingId);

        if (!string.IsNullOrEmpty(siteSubscription.ExternalProductId) && model.Amount > 0)
        {
            price.ExternalId = await paymentProvider.CreateSubscriptionPlan(
                new ExternalSubscriptionPlan
                {
                    Amount = model.Amount,
                    CurrencyCode = currency.Code,
                    ExternalId = string.Empty,
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

    public async Task DeleteSiteSubscriptionPrice(
        IMemberServiceRequest request, Guid siteSubscriptionId, Guid siteSubscriptionPriceId)
    {
        var (sitePaymentSettings, siteSubscription, price) = await GetSiteAdminRestrictedContent(request,
            x => x.SitePaymentSettingsRepository.GetAll(),
            x => x.SiteSubscriptionRepository.GetById(siteSubscriptionId),
            x => x.SiteSubscriptionPriceRepository.GetById(siteSubscriptionPriceId));

        OdkAssertions.MeetsCondition(price, x => x.SiteSubscriptionId == siteSubscriptionId);

        _unitOfWork.SiteSubscriptionPriceRepository.Delete(price);
        await _unitOfWork.SaveChangesAsync();

        if (!string.IsNullOrEmpty(price.ExternalId))
        {
            var paymentProvider = _paymentProviderFactory.GetSitePaymentProvider(
                sitePaymentSettings,
                siteSubscription.SitePaymentSettingId);
            await paymentProvider.DeactivateSubscriptionPlan(price.ExternalId);
        }
    }

    public async Task<IReadOnlyCollection<SiteSubscription>> GetAllSubscriptions(
        IMemberServiceRequest request)
    {
        var platform = request.Platform;

        return await GetSiteAdminRestrictedContent(request,
            x => x.SiteSubscriptionRepository.GetAll(platform));
    }

    public async Task<IReadOnlyCollection<SiteSubscriptionSiteAdminListItemViewModel>> GetSiteSubscriptionSiteAdminListItems(
        IMemberServiceRequest request)
    {
        var platform = request.Platform;

        var (sitePaymentSettings, siteSubscriptionSummaries, prices) = await GetSiteAdminRestrictedContent(request,
            x => x.SitePaymentSettingsRepository.GetAll(),
            x => x.SiteSubscriptionRepository.GetSummaries(platform),
            x => x.SiteSubscriptionPriceRepository.GetAll(platform));

        var priceDictionary = prices
            .GroupBy(x => x.SiteSubscriptionId)
            .ToDictionary(x => x.Key, x => x.ToArray());

        var siteSubscriptionDictionary = siteSubscriptionSummaries
            .ToDictionary(x => x.SiteSubscription.Id);

        var sitePaymentSettingsDictionary = sitePaymentSettings
            .ToDictionary(x => x.Id);

        return siteSubscriptionSummaries
            .Select(x => new SiteSubscriptionSiteAdminListItemViewModel
            {
                ActiveCount = x.ActiveMemberSiteSubscriptionCount,
                Default = x.SiteSubscription.Default,
                Enabled = x.SiteSubscription.Enabled,
                Features = x.SiteSubscription.Features.Select(x => x.Feature).ToArray(),
                GroupLimit = x.SiteSubscription.GroupLimit,
                Id = x.SiteSubscription.Id,
                MemberLimit = x.SiteSubscription.MemberLimit,
                Name = x.SiteSubscription.Name,
                PaymentSettingsName = sitePaymentSettingsDictionary[x.SiteSubscription.SitePaymentSettingId].Name,
                Prices = priceDictionary.TryGetValue(x.SiteSubscription.Id, out var prices)
                    ? prices
                        .Select(x => new SiteSubscriptionSiteAdminListItemPriceViewModel
                        {
                            Amount = x.Amount,
                            Currency = x.Currency,
                            Frequency = x.Frequency
                        })
                        .OrderBy(x => x.Currency.Code)
                        .ThenBy(x => x.Amount)
                        .ToArray()
                    : []
            })
            .OrderBy(x => x.PaymentSettingsName)
            .ThenBy(x => x.Name)
            .ToArray();
    }

    public async Task<SiteSubscriptionViewModel> GetSubscriptionViewModel(
        IMemberServiceRequest request, Guid siteSubscriptionId)
    {
        var (subscription, prices, currencies, sitePaymentSettings) = await GetSiteAdminRestrictedContent(request,
            x => x.SiteSubscriptionRepository.GetById(siteSubscriptionId),
            x => x.SiteSubscriptionPriceRepository.GetBySiteSubscriptionId(siteSubscriptionId),
            x => x.CurrencyRepository.GetAll(),
            x => x.SitePaymentSettingsRepository.GetAll());

        return new SiteSubscriptionViewModel
        {
            Currencies = currencies,
            CurrentMemberExternalSubscription = null,
            CurrentMemberSiteSubscription = null,
            Prices = prices,
            SitePaymentSettings = sitePaymentSettings,
            Subscription = subscription
        };
    }

    public async Task MakeDefault(IMemberServiceRequest request, Guid siteSubscriptionId)
    {
        var platform = request.Platform;

        var subscriptions = await GetSiteAdminRestrictedContent(request,
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
        IMemberServiceRequest request, Guid siteSubscriptionId, SiteSubscriptionCreateModel model)
    {
        var platform = request.Platform;

        var existing = await GetSiteAdminRestrictedContent(request,
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

    public async Task<ServiceResult> UpdateSiteSubscriptionEnabled(
        IMemberServiceRequest request, Guid siteSubscriptionId, bool enabled)
    {
        var subscription = await GetSiteAdminRestrictedContent(request,
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
        subscription.Name = model.Name;

        if (subscription.Features == null)
        {
            subscription.Features = new List<SiteSubscriptionFeature>();
        }

        var existingFeatures = subscription.Features.Select(x => x.Feature).ToHashSet();
        var modelFeatures = model.Features.ToHashSet();

        // add new features
        foreach (var feature in modelFeatures)
        {
            if (!existingFeatures.Contains(feature))
            {
                _unitOfWork.SiteSubscriptionFeatureRepository.Add(new SiteSubscriptionFeature
                {
                    Feature = feature,
                    Id = Guid.NewGuid(),
                    SiteSubscriptionId = subscription.Id
                });
            }
        }

        // remove old features
        foreach (var feature in existingFeatures)
        {
            if (!modelFeatures.Contains(feature))
            {
                var siteSubscriptionFeature = subscription.Features.First(x => x.Feature == feature);
                _unitOfWork.SiteSubscriptionFeatureRepository.Delete(siteSubscriptionFeature);
            }
        }
    }
}