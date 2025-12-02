using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Subscriptions;
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
    private readonly IPaymentService _paymentService;
    private readonly IUnitOfWork _unitOfWork;

    public SiteSubscriptionService(
        IUnitOfWork unitOfWork, 
        IPaymentService paymentService,
        IMemberEmailService memberEmailService,
        ILoggingService loggingService)
    { 
        _loggingService = loggingService;
        _memberEmailService = memberEmailService;
        _paymentService = paymentService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> ConfirmMemberSiteSubscription(
        MemberServiceRequest request, 
        Guid siteSubscriptionPriceId, 
        string externalId)
    {
        var (memberId, platform) = (request.CurrentMemberId, request.Platform);

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

    public async Task<PaymentStatusType> GetMemberSiteSubscriptionPaymentCheckoutSessionStatus(
        MemberServiceRequest request, string externalSessionId)
    {
        var (sitePaymentSettings, checkoutSession) = await _unitOfWork.RunAsync(
            x => x.SitePaymentSettingsRepository.GetActive(),
            x => x.PaymentCheckoutSessionRepository.GetByMemberId(request.CurrentMemberId, externalSessionId));
        
        if (checkoutSession.CompletedUtc != null)
        {
            return PaymentStatusType.Complete;
        }

        var externalSession = await _paymentService.GetCheckoutSession(sitePaymentSettings, externalSessionId);

        return externalSession == null
            ? PaymentStatusType.Expired
            : PaymentStatusType.Pending;
    }

    public async Task<SiteSubscriptionsViewModel> GetSiteSubscriptionsViewModel(
        ServiceRequest request, Guid? memberId)
    {        
        var platform = request.Platform;

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

        await _loggingService.Info(
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
        MemberServiceRequest request, Guid priceId, string returnPath)
    {
        var (memberId, platform) = (request.CurrentMemberId, request.Platform);

        var (member, siteSubscription, price, paymentSettings) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(memberId),
            x => x.SiteSubscriptionRepository.GetByPriceId(priceId),
            x => x.SiteSubscriptionPriceRepository.GetById(priceId),
            x => x.SitePaymentSettingsRepository.GetActive());

        if (string.IsNullOrEmpty(price.ExternalId))
        {
            throw new Exception("Error starting checkout session: siteSubscriptionPrice.ExternalId missing");
        }

        var externalSubscriptionPlan = await _paymentService.GetSubscriptionPlan(paymentSettings, price.ExternalId);
        if (externalSubscriptionPlan == null)
        {
            throw new Exception("Error starting checkout session: subscriptionPlan not found");
        }

        var utcNow = DateTime.UtcNow;
        var paymentCheckoutSessionId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();

        var metadata = new PaymentMetadataModel(
            member, 
            price, 
            paymentCheckoutSessionId: paymentCheckoutSessionId, 
            paymentId: paymentId);

        var externalCheckoutSession = await _paymentService.StartCheckoutSession(
            request, paymentSettings, externalSubscriptionPlan, returnPath, metadata);

        _unitOfWork.PaymentCheckoutSessionRepository.Add(new PaymentCheckoutSession
        {
            Id = paymentCheckoutSessionId,
            MemberId = memberId,
            PaymentId = priceId,
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
            MemberId = memberId,
            Reference = siteSubscription.ToReference()
        });

        await _unitOfWork.SaveChangesAsync();

        return new SiteSubscriptionCheckoutViewModel
        {
            ClientSecret = externalCheckoutSession.ClientSecret,
            CurrencyCode = externalSubscriptionPlan.CurrencyCode,
            PaymentSettings = paymentSettings,
            Platform = platform
        };
    }

    public async Task SyncExpiredSubscriptions(ServiceRequest request)
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

    public async Task<ServiceResult> UpdateMemberSiteSubscription(
        MemberServiceRequest request,
        Guid siteSubscriptionId,
        SiteSubscriptionFrequency frequency)
    {
        var (memberId, platform) = (request.CurrentMemberId, request.Platform);

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
