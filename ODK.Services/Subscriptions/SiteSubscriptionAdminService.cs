﻿using System.Reflection;
using ODK.Core;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Services.Payments;
using ODK.Services.Subscriptions.ViewModels;

namespace ODK.Services.Subscriptions;

public class SiteSubscriptionAdminService : OdkAdminServiceBase, ISiteSubscriptionAdminService
{
    private readonly IHtmlSanitizer _htmlSanitizer;
    private readonly IPaymentService _paymentService;
    private readonly IPlatformProvider _platformProvider;
    private readonly IUnitOfWork _unitOfWork;

    public SiteSubscriptionAdminService(
        IUnitOfWork unitOfWork,
        IPlatformProvider platformProvider,
        IPaymentService paymentService,
        IHtmlSanitizer htmlSanitizer) 
        : base(unitOfWork)
    {
        _htmlSanitizer = htmlSanitizer;
        _paymentService = paymentService;
        _platformProvider = platformProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> AddSiteSubscription(Guid currentMemberId, SiteSubscriptionCreateModel model)
    {
        var platform = _platformProvider.GetPlatform();
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
            Platform = _platformProvider.GetPlatform(),
            SitePaymentSettingId = model.SitePaymentSettingId
        };

        UpdateSiteSubscription(model, subscription);

        subscription.ExternalProductId = await _paymentService.CreateProduct(paymentSettings, subscription.Name);

        _unitOfWork.SiteSubscriptionRepository.Add(subscription);

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> AddSiteSubscriptionPrice(Guid currentMemberId, Guid siteSubscriptionId,
        SiteSubscriptionPriceCreateModel model)
    {
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

        if (!string.IsNullOrEmpty(siteSubscription.ExternalProductId) && model.Amount > 0)
        {
            price.ExternalId = await _paymentService.CreateSubscriptionPlan(siteSubscription.SitePaymentSettings, new ExternalSubscriptionPlan
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
            await _paymentService.ActivateSubscriptionPlan(siteSubscription.SitePaymentSettings, price.ExternalId);
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
            await _paymentService.DeactivateSubscriptionPlan(siteSubscription.SitePaymentSettings, price.ExternalId);
        }
    }

    public async Task<IReadOnlyCollection<SiteSubscription>> GetAllSubscriptions(Guid currentMemberId)
    {
        var platform = _platformProvider.GetPlatform();
        return await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.SiteSubscriptionRepository.GetAll(platform));
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

    public async Task MakeDefault(Guid currentMemberId, Guid siteSubscriptionId)
    {
        var platform = _platformProvider.GetPlatform();

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

    public async Task<ServiceResult> UpdateSiteSubscription(Guid currentMemberId, Guid siteSubscriptionId, SiteSubscriptionCreateModel model)
    {
        var platform = _platformProvider.GetPlatform();

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
