using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.Payments;
using ODK.Services.Payments.Models;
using ODK.Services.Subscriptions.ViewModels;

namespace ODK.Services.Subscriptions;

public class SiteSubscriptionService : ISiteSubscriptionService
{
    private readonly ILoggingService _loggingService;
    private readonly IMemberEmailService _memberEmailService;
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly IUnitOfWork _unitOfWork;

    public SiteSubscriptionService(
        IUnitOfWork unitOfWork,
        IMemberEmailService memberEmailService,
        ILoggingService loggingService,
        IPaymentProviderFactory paymentProviderFactory)
    {
        _loggingService = loggingService;
        _memberEmailService = memberEmailService;
        _paymentProviderFactory = paymentProviderFactory;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> CancelMemberSiteSubscription(
        IMemberServiceRequest request, Guid siteSubscriptionId)
    {
        var (currentMember, platform) = (request.CurrentMember, request.Platform);

        var (memberSubscription, sitePaymentSettings) = await _unitOfWork.RunAsync(
            x => x.MemberSiteSubscriptionRepository.GetByIdOrDefault(siteSubscriptionId),
            x => x.SitePaymentSettingsRepository.GetAll());

        OdkAssertions.BelongsToMember(memberSubscription, currentMember.Id);

        if (memberSubscription == null)
        {
            return ServiceResult.Failure("Subscription not found");
        }

        if (string.IsNullOrEmpty(memberSubscription.ExternalId))
        {
            return ServiceResult.Failure("External subscription not found");
        }

        var paymentProvider = _paymentProviderFactory.GetSitePaymentProvider(
            sitePaymentSettings,
            memberSubscription.SiteSubscription.SitePaymentSettingId);

        var result = await paymentProvider.CancelSubscription(memberSubscription.ExternalId);

        return result
            ? ServiceResult.Successful()
            : ServiceResult.Failure("Subscription could not be cancelled");
    }

    public async Task<ServiceResult> ConfirmMemberSiteSubscription(
        IMemberServiceRequest request,
        Guid siteSubscriptionPriceId,
        string externalId)
    {
        var (currentMember, platform) = (request.CurrentMember, request.Platform);

        var (sitePaymentSettings, memberSubscription, siteSubscriptionPrice) = await _unitOfWork.RunAsync(
            x => x.SitePaymentSettingsRepository.GetAll(),
            x => x.MemberSiteSubscriptionRepository.GetByMemberId(currentMember.Id, platform),
            x => x.SiteSubscriptionPriceRepository.GetById(siteSubscriptionPriceId));

        var siteSubscription = await _unitOfWork.SiteSubscriptionRepository.GetById(siteSubscriptionPrice.SiteSubscriptionId).Run();

        var paymentProvider = _paymentProviderFactory.GetSitePaymentProvider(
            sitePaymentSettings,
            memberSubscription.SiteSubscription.SitePaymentSettingId);

        var externalSubscription = await paymentProvider.GetSubscription(externalId);

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
            memberSubscription.MemberId = currentMember.Id;
            _unitOfWork.MemberSiteSubscriptionRepository.Add(memberSubscription);
        }
        else
        {
            _unitOfWork.MemberSiteSubscriptionRepository.Update(memberSubscription);
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<SiteSubscriptionsViewModel> GetSiteSubscriptionsViewModel(
        IServiceRequest request, Guid? memberId, Guid? chapterId)
    {
        var platform = request.Platform;

        var (sitePaymentSettings,
            subscriptions,
            prices,
            currentMember,
            memberSubscription,
            memberCurrency,
            chapterCurrency) = await _unitOfWork.RunAsync(
            x => x.SitePaymentSettingsRepository.GetAll(),
            x => x.SiteSubscriptionRepository.GetAllEnabled(platform),
            x => x.SiteSubscriptionPriceRepository.GetAllEnabled(platform),
            x => memberId != null
                ? x.MemberRepository.GetByIdOrDefault(memberId.Value)
                : new DefaultDeferredQuerySingleOrDefault<Member>(),
            x => memberId != null
                ? x.MemberSiteSubscriptionRepository.GetByMemberId(memberId.Value, platform)
                : new DefaultDeferredQuerySingleOrDefault<MemberSiteSubscription>(),
            x => memberId != null
                ? x.CurrencyRepository.GetByMemberIdOrDefault(memberId.Value)
                : new DefaultDeferredQuerySingleOrDefault<Currency>(),
            x => chapterId != null
                ? x.CurrencyRepository.GetByChapterIdOrDefault(chapterId.Value)
                : new DefaultDeferredQuerySingleOrDefault<Currency>());

        var currency = memberCurrency ?? chapterCurrency;

        var currencies = prices
            .Where(x => currency == null || x.CurrencyId == currency.Id)
            .GroupBy(x => x.CurrencyId)
            .Select(x => x.First().Currency)
            .ToArray();

        var priceDictionary = prices
            .Where(x => currency == null || x.CurrencyId == currency.Id || x.Amount == 0)
            .GroupBy(x => x.SiteSubscriptionId)
            .ToDictionary(x => x.Key, x => x.ToArray());

        var sitePaymentSettingsDictionary = sitePaymentSettings
            .ToDictionary(x => x.Id);

        var siteSubscriptionViewModels = subscriptions
            .Where(x => x.IsEnabled(sitePaymentSettingsDictionary[x.SitePaymentSettingId]))
            .Select(x => new SiteSubscriptionViewModel
            {
                Currencies = [],
                Prices = priceDictionary.ContainsKey(x.Id) ? priceDictionary[x.Id] : [],
                SitePaymentSettings = sitePaymentSettings,
                Subscription = x
            })
            .ToArray();

        var externalSubscription = await GetExternalSubscription(sitePaymentSettings, memberSubscription);

        return new SiteSubscriptionsViewModel
        {
            Currencies = currencies,
            Currency = currency,
            CurrentMember = currentMember,
            CurrentMemberSubscription = memberSubscription,
            CurrentMemberExternalSubscription = externalSubscription,
            SitePaymentSettings = subscriptions
                .Select(x => sitePaymentSettingsDictionary[x.SitePaymentSettingId])
                .GroupBy(x => x.Id)
                .Select(x => x.First())
                .ToArray(),
            Subscriptions = siteSubscriptionViewModels
                .Where(x => x.Prices.Count > 0)
                .ToArray()
        };
    }

    public async Task<SiteSubscriptionCheckoutViewModel> StartSiteSubscriptionCheckout(
        IMemberServiceRequest request, Guid priceId, string returnPath, Guid? chapterId)
    {
        var (platform, currentMember) = (request.Platform, request.CurrentMember);

        var (siteSubscription, price, chapter) = await _unitOfWork.RunAsync(
            x => x.SiteSubscriptionRepository.GetByPriceId(priceId),
            x => x.SiteSubscriptionPriceRepository.GetById(priceId),
            x => chapterId != null
                ? x.ChapterRepository.GetByIdOrDefault(platform, chapterId.Value)
                : new DefaultDeferredQuerySingleOrDefault<Chapter>());

        if (string.IsNullOrEmpty(price.ExternalId))
        {
            throw new Exception("Error starting checkout session: siteSubscriptionPrice.ExternalId missing");
        }

        var sitePaymentSettings = await _unitOfWork.SitePaymentSettingsRepository
            .GetById(siteSubscription.SitePaymentSettingId).Run();

        var paymentProvider = _paymentProviderFactory.GetSitePaymentProvider(sitePaymentSettings);

        var externalSubscriptionPlan = await paymentProvider.GetSubscriptionPlan(price.ExternalId);
        if (externalSubscriptionPlan == null)
        {
            throw new Exception("Error starting checkout session: subscriptionPlan not found");
        }

        var utcNow = DateTime.UtcNow;
        var paymentCheckoutSessionId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();

        var metadata = new PaymentMetadataModel(
            PaymentReasonType.SiteSubscription,
            currentMember,
            price,
            paymentCheckoutSessionId: paymentCheckoutSessionId,
            paymentId: paymentId);

        var externalCheckoutSession = await paymentProvider.StartCheckout(
            request,
            currentMember.EmailAddress,
            externalSubscriptionPlan,
            returnPath,
            metadata);

        _unitOfWork.PaymentCheckoutSessionRepository.Add(new PaymentCheckoutSession
        {
            Id = paymentCheckoutSessionId,
            MemberId = currentMember.Id,
            PaymentId = paymentId,
            SessionId = externalCheckoutSession.SessionId,
            StartedUtc = utcNow
        });

        _unitOfWork.PaymentRepository.Add(new Payment
        {
            Amount = price.Amount,
            CreatedUtc = utcNow,
            CurrencyId = price.CurrencyId,
            ExternalId = externalCheckoutSession.PaymentId,
            Id = paymentId,
            MemberId = currentMember.Id,
            Reference = siteSubscription.ToReference(),
            SitePaymentSettingId = siteSubscription.SitePaymentSettingId
        });

        await _unitOfWork.SaveChangesAsync();

        return new SiteSubscriptionCheckoutViewModel
        {
            Chapter = chapter,
            ClientSecret = externalCheckoutSession.ClientSecret,
            PaymentSettings = sitePaymentSettings,
            Platform = platform
        };
    }

    public async Task SyncExpiredSubscriptions(IServiceRequest request)
    {
        var (sitePaymentSettings, subscriptions) = await _unitOfWork.RunAsync(
            x => x.SitePaymentSettingsRepository.GetAll(),
            x => x.MemberSiteSubscriptionRepository.GetExpired());

        var updated = new List<MemberSiteSubscription>();

        foreach (var subscription in subscriptions)
        {
            if (string.IsNullOrEmpty(subscription.ExternalId))
            {
                continue;
            }

            var paymentProvider = _paymentProviderFactory.GetSitePaymentProvider(
                sitePaymentSettings,
                subscription.SiteSubscription.SitePaymentSettingId);

            var externalSubscription = await paymentProvider.GetSubscription(subscription.ExternalId);
            if (externalSubscription?.NextBillingDate > DateTime.UtcNow)
            {
                subscription.ExpiresUtc = externalSubscription.NextBillingDate.Value;
                updated.Add(subscription);
            }
            else
            {
                var member = await _unitOfWork.MemberRepository.GetById(subscription.MemberId).Run();
                await _memberEmailService.SendSiteSubscriptionExpiredEmail(request, member);
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

    private async Task<ExternalSubscription?> GetExternalSubscription(
        IReadOnlyCollection<SitePaymentSettings> sitePaymentSettings,
        MemberSiteSubscription? memberSubscription)
    {
        if (string.IsNullOrEmpty(memberSubscription?.ExternalId))
        {
            return null;
        }

        var paymentProvider = _paymentProviderFactory.GetSitePaymentProvider(
            sitePaymentSettings,
            memberSubscription.SiteSubscription.SitePaymentSettingId);

        return await paymentProvider.GetSubscription(memberSubscription.ExternalId);
    }
}