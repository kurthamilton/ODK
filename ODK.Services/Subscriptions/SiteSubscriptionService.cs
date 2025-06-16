﻿using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.Payments;
using ODK.Services.Subscriptions.ViewModels;

namespace ODK.Services.Subscriptions;

public class SiteSubscriptionService : ISiteSubscriptionService
{
    private readonly ILoggingService _loggingService;
    private readonly IMemberEmailService _memberEmailService;
    private readonly IPaymentService _paymentService;
    private readonly IPlatformProvider _platformProvider;
    private readonly IUnitOfWork _unitOfWork;

    public SiteSubscriptionService(
        IUnitOfWork unitOfWork, 
        IPlatformProvider platformProvider,
        IPaymentService paymentService,
        IMemberEmailService memberEmailService,
        ILoggingService loggingService)
    { 
        _loggingService = loggingService;
        _memberEmailService = memberEmailService;
        _paymentService = paymentService;
        _platformProvider = platformProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> CompleteSiteSubscriptionCheckoutSession(
        Guid memberId, Guid siteSubscriptionPriceId, string sessionId)
    {
        var platform = _platformProvider.GetPlatform();

        var (member, siteSubscriptionPrice, paymentCheckoutSession) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(memberId),
            x => x.SiteSubscriptionPriceRepository.GetById(siteSubscriptionPriceId),
            x => x.PaymentCheckoutSessionRepository.GetByMemberId(memberId, sessionId));

        var siteSubscription = await _unitOfWork.SiteSubscriptionRepository
            .GetById(siteSubscriptionPrice.SiteSubscriptionId)
            .Run();

        if (paymentCheckoutSession == null || paymentCheckoutSession.CompletedUtc != null)
        {
            return false;
        }

        var (paymentSettings, memberSubscription) = await _unitOfWork.RunAsync(
            x => x.SitePaymentSettingsRepository.GetActive(),
            x => x.MemberSiteSubscriptionRepository.GetByMemberId(memberId, platform));

        if (string.IsNullOrEmpty(siteSubscriptionPrice.ExternalId))
        {
            throw new Exception("Error completing checkout session: siteSubscriptionPrice.ExternalId missing");
        }

        var subscriptionPlan = await _paymentService.GetSubscriptionPlan(
            paymentSettings, siteSubscriptionPrice.ExternalId);
        if (subscriptionPlan == null)
        {
            throw new Exception("Error completing checkout session: subscriptionPlan not found");
        }

        var checkoutSession = await _paymentService.GetCheckoutSession(paymentSettings, sessionId);
        if (checkoutSession?.Complete != true)
        {
            return false;
        }

        memberSubscription ??= new MemberSiteSubscription();

        var now = memberSubscription.ExpiresUtc > DateTime.UtcNow 
            ? memberSubscription.ExpiresUtc.Value 
            : DateTime.UtcNow;
        var months = siteSubscriptionPrice.Frequency == SiteSubscriptionFrequency.Yearly
            ? 12
            : 1;

        var expiresUtc = now.AddMonths(months);
        memberSubscription.ExpiresUtc = expiresUtc;
        memberSubscription.SiteSubscriptionPriceId = siteSubscriptionPrice.Id;
        memberSubscription.SiteSubscriptionId = siteSubscriptionPrice.SiteSubscriptionId;
        memberSubscription.PaymentProvider = paymentSettings.Provider;

        if (memberSubscription.Id == default)
        {
            memberSubscription.Id = Guid.NewGuid();
            memberSubscription.MemberId = memberId;            
            _unitOfWork.MemberSiteSubscriptionRepository.Add(memberSubscription);
        }
        else
        {
            _unitOfWork.MemberSiteSubscriptionRepository.Update(memberSubscription);
        }

        paymentCheckoutSession.CompletedUtc = DateTime.UtcNow;
        _unitOfWork.PaymentCheckoutSessionRepository.Update(paymentCheckoutSession);

        var payment = new Payment
        {
            Amount = siteSubscriptionPrice.Amount,
            ChapterId = null,
            CurrencyId = siteSubscriptionPrice.CurrencyId,
            Id = Guid.NewGuid(),
            MemberId = memberId,
            PaidUtc = DateTime.UtcNow,
            Reference = $"Subscription: {siteSubscription.Name}"
        };

        _unitOfWork.PaymentRepository.Add(payment);

        await _unitOfWork.SaveChangesAsync();

        var siteEmailSettings = await _unitOfWork.SiteEmailSettingsRepository.Get(platform).Run();
        var currency = siteSubscriptionPrice.Currency;

        await _memberEmailService.SendPaymentNotification(payment, currency, siteEmailSettings);

        return true;
    }

    public async Task<ServiceResult> ConfirmMemberSiteSubscription(
        Guid memberId, 
        Guid siteSubscriptionPriceId, 
        string externalId)
    {
        var platform = _platformProvider.GetPlatform();

        var (memberSubscription, siteSubscriptionPrice) = await _unitOfWork.RunAsync(
            x => x.MemberSiteSubscriptionRepository.GetByMemberId(memberId, platform),
            x => x.SiteSubscriptionPriceRepository.GetById(siteSubscriptionPriceId));

        var siteSubscription = await _unitOfWork.SiteSubscriptionRepository.GetById(siteSubscriptionPrice.SiteSubscriptionId).Run();

        var externalSubscription = await _paymentService.GetSubscription(siteSubscription.SitePaymentSettings, externalId);

        if (externalSubscription == null || 
            externalSubscription.ExternalSubscriptionPlanId != siteSubscriptionPrice.ExternalId)
        {
            return ServiceResult.Failure("Error confirming subscription");
        }        

        memberSubscription ??= new MemberSiteSubscription();        
        
        memberSubscription.ExternalId = externalSubscription.ExternalId;
        memberSubscription.ExpiresUtc = externalSubscription.NextBillingDate;

        if (memberSubscription.SiteSubscriptionId != siteSubscriptionPrice.SiteSubscriptionId)
        {
            memberSubscription.SiteSubscriptionId = siteSubscription.Id;
            memberSubscription.SiteSubscriptionPriceId = siteSubscriptionPrice.Id;
        }

        if (memberSubscription.MemberId == default)
        {
            memberSubscription.MemberId = memberId;
            _unitOfWork.MemberSiteSubscriptionRepository.Add(memberSubscription);
        }
        else
        {
            _unitOfWork.MemberSiteSubscriptionRepository.Update(memberSubscription);
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<MemberSiteSubscription?> GetMemberSiteSubscription(Guid memberId)
    {
        var platform = _platformProvider.GetPlatform();
        return await _unitOfWork.MemberSiteSubscriptionRepository
            .GetByMemberId(memberId, platform)
            .Run();
    }

    public async Task<SiteSubscriptionsViewModel> GetSiteSubscriptionsViewModel(Guid? memberId)
    {        
        var platform = _platformProvider.GetPlatform();

        var (subscriptions, prices, currentMember, memberPaymentSettings, memberSubscription) = await _unitOfWork.RunAsync(
            x => x.SiteSubscriptionRepository.GetAllEnabled(platform),
            x => x.SiteSubscriptionPriceRepository.GetAllEnabled(platform),
            x => memberId != null
                ? x.MemberRepository.GetByIdOrDefault(memberId.Value)
                : new DefaultDeferredQuerySingleOrDefault<Member>(),
            x => memberId != null 
                ? x.MemberPaymentSettingsRepository.GetByMemberId(memberId.Value)
                : new DefaultDeferredQuerySingleOrDefault<MemberPaymentSettings>(),
            x => memberId != null
                ? x.MemberSiteSubscriptionRepository.GetByMemberId(memberId.Value, platform)
                : new DefaultDeferredQuerySingleOrDefault<MemberSiteSubscription>());        

        var currency = memberPaymentSettings?.Currency;

        var currencies = prices
            .Where(x => currency == null || x.CurrencyId == currency.Id)
            .GroupBy(x => x.CurrencyId)
            .Select(x => x.First().Currency)
            .ToArray();

        var priceDictionary = prices
            .Where(x => currency == null || x.CurrencyId == currency.Id || x.Amount == 0)
            .GroupBy(x => x.SiteSubscriptionId)
            .ToDictionary(x => x.Key, x => x.ToArray());

        await _loggingService.LogDebug(
            $"Getting site subscription view models: " +
            $"found {subscriptions.Count} total subscriptions, " +
            $"found {subscriptions.Count(x => x.SitePaymentSettings.Active)} active subscriptions");

        var siteSubscriptionViewModels = subscriptions
            .Select(x => new SiteSubscriptionViewModel
            {
                Currencies = [],
                Prices = priceDictionary.ContainsKey(x.Id) ? priceDictionary[x.Id] : [],
                SitePaymentSettings = [],
                Subscription = x
            })
            .ToArray();

        return new SiteSubscriptionsViewModel
        {
            Currencies = currencies,            
            Currency = currency,
            CurrentMember = currentMember,
            CurrentMemberSubscription = memberSubscription,
            PaymentSettings = subscriptions
                .Select(x => x.SitePaymentSettings)
                .GroupBy(x => x.Id)
                .Select(x => x.First())
                .ToArray(),
            Subscriptions = siteSubscriptionViewModels
        };
    }

    public async Task<SiteSubscriptionCheckoutViewModel> StartSiteSubscriptionCheckout(
        Guid memberId, Guid priceId, string returnPath)
    {
        var platform = _platformProvider.GetPlatform();

        var (member, price, paymentSettings) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(memberId),
            x => x.SiteSubscriptionPriceRepository.GetById(priceId),
            x => x.SitePaymentSettingsRepository.GetActive());

        if (string.IsNullOrEmpty(price.ExternalId))
        {
            throw new Exception("Error starting checkout session: siteSubscriptionPrice.ExternalId missing");
        }

        var subscriptionPlan = await _paymentService.GetSubscriptionPlan(paymentSettings, price.ExternalId);
        if (subscriptionPlan == null)
        {
            throw new Exception("Error starting checkout session: subscriptionPlan not found");
        }

        var session = await _paymentService.StartCheckoutSession(paymentSettings, subscriptionPlan, returnPath);

        _unitOfWork.PaymentCheckoutSessionRepository.Add(new PaymentCheckoutSession
        {
            MemberId = memberId,
            PaymentId = priceId,
            SessionId = session.SessionId,
            StartedUtc = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

        return new SiteSubscriptionCheckoutViewModel
        {
            ClientSecret = session.ClientSecret,
            CurrencyCode = subscriptionPlan.CurrencyCode,
            PaymentSettings = paymentSettings,
            Platform = platform
        };
    }

    public async Task SyncExpiredSubscriptions()
    {
        var subscriptions = await _unitOfWork.MemberSiteSubscriptionRepository.GetExpired().Run();

        var updated = new List<MemberSiteSubscription>();

        foreach (var subscription in subscriptions)
        {
            if (string.IsNullOrEmpty(subscription.ExternalId))
            {
                continue;
            }

            var paymentSettings = subscription.SiteSubscription.SitePaymentSettings;
            var externalSubscription = await _paymentService.GetSubscription(paymentSettings, subscription.ExternalId);
            if (externalSubscription?.NextBillingDate > DateTime.UtcNow)
            {                
                subscription.ExpiresUtc = externalSubscription.NextBillingDate.Value;
                updated.Add(subscription);
            }
            else
            {
                var member = await _unitOfWork.MemberRepository.GetById(subscription.MemberId).Run();
                await _memberEmailService.SendSiteSubscriptionExpiredEmail(member);
            }
        }

        if (updated.Count > 0)
        {
            foreach (var subscription in updated)
            {
                _unitOfWork.MemberSiteSubscriptionRepository.Update(subscription);
            }

            await _unitOfWork.SaveChangesAsync();
        }        
    }

    public async Task<ServiceResult> UpdateMemberSiteSubscription(Guid memberId, Guid siteSubscriptionId,
        SiteSubscriptionFrequency frequency)
    {
        var platform = _platformProvider.GetPlatform();

        var (memberPaymentSettings, siteSubscription, memberSubscription) = await _unitOfWork.RunAsync(
                x => x.MemberPaymentSettingsRepository.GetByMemberId(memberId),
                x => x.SiteSubscriptionPriceRepository.GetBySiteSubscriptionId(siteSubscriptionId),
                x => x.MemberSiteSubscriptionRepository.GetByMemberId(memberId, platform));

        IPaymentSettings? paymentSettings = memberPaymentSettings;

        if (memberPaymentSettings == null || !paymentSettings.HasApiKey)
        {
            paymentSettings = await GetChapterPaymentSettingsByOwnerId(memberId);
        }

        if (paymentSettings == null || !paymentSettings.HasApiKey)
        {
            return ServiceResult.Failure("Automated subscriptions cannot be set up - missing payment settings");
        }

        return ServiceResult.Failure("Not set up");
    }

    private async Task<ChapterPaymentSettings?> GetChapterPaymentSettingsByOwnerId(Guid ownerId)
    {
        var chapters = await _unitOfWork.ChapterRepository.GetByOwnerId(ownerId).Run();
        if (chapters.Count == 0)
        {
            return null;
        }

        foreach (var chapter in chapters)
        {
            var chapterPaymentSettings = await _unitOfWork.ChapterPaymentSettingsRepository
                .GetByChapterId(chapter.Id)
                .Run();

            if (chapterPaymentSettings != null)
            {
                return chapterPaymentSettings;
            }
        }

        return null;
    }
}
