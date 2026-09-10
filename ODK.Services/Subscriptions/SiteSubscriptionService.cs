using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Subscriptions;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Members;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.Notifications;
using ODK.Services.Payments;
using ODK.Services.Payments.Models;
using ODK.Services.Subscriptions.ViewModels;

namespace ODK.Services.Subscriptions;

public class SiteSubscriptionService : ISiteSubscriptionService
{
    private readonly ILoggingService _loggingService;
    private readonly IMemberEmailService _memberEmailService;
    private readonly IMemberSiteSubscriptionWriter _memberSiteSubscriptionWriter;
    private readonly INotificationService _notificationService;
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly IPaymentService _paymentService;
    private readonly SiteSubscriptionCooldown _siteSubscriptionCooldown;
    private readonly IUnitOfWork _unitOfWork;

    public SiteSubscriptionService(
        IUnitOfWork unitOfWork,
        IPaymentProviderFactory paymentProviderFactory,
        IPaymentService paymentService,
        IMemberSiteSubscriptionWriter memberSiteSubscriptionWriter,
        INotificationService notificationService,
        IMemberEmailService memberEmailService,
        ILoggingService loggingService,
        SiteSubscriptionCooldown siteSubscriptionCooldown)
    {
        _loggingService = loggingService;
        _memberEmailService = memberEmailService;
        _memberSiteSubscriptionWriter = memberSiteSubscriptionWriter;
        _notificationService = notificationService;
        _paymentProviderFactory = paymentProviderFactory;
        _paymentService = paymentService;
        _siteSubscriptionCooldown = siteSubscriptionCooldown;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> CancelMemberSiteSubscription(IMemberServiceRequest request)
    {
        var currentMember = request.CurrentMember;

        /* The member's most recent record naming an external subscription, which need not be their current
           one: a lapsed subscription is downgraded onto the free plan, and the record that replaces it
           carries no external id. Reading the current record would refuse to cancel a provider subscription
           that is still live. */
        var memberSubscriptionDto = await _unitOfWork.Run(
            x => x.MemberSiteSubscriptionRecordRepository
                .Query()
                .ForMember(currentMember.Id)
                .HasExternalId()
                .MostRecent()
                .ToDto()
                .GetSingleOrDefault());

        var externalId = memberSubscriptionDto?.MemberSiteSubscription.ExternalId;
        if (memberSubscriptionDto == null || string.IsNullOrEmpty(externalId))
        {
            return ServiceResult.Failure("External subscription not found");
        }

        OdkAssertions.MeetsCondition(memberSubscriptionDto.MemberSiteSubscription, x => x.MemberId == currentMember.Id);

        var siteSubscription = memberSubscriptionDto.SiteSubscription;
        var paymentProvider = _paymentProviderFactory.GetPaymentProvider(
            siteSubscription.PaymentProvider, siteSubscription.Platform);

        var result = await paymentProvider.CancelSubscription(externalId);

        return result
            ? ServiceResult.Successful()
            : ServiceResult.Failure("Subscription could not be cancelled");
    }

    public async Task DowngradeLapsedSubscriptions(IServiceRequest request)
    {
        var (environment, platform) = (request.Environment, request.Platform);

        var (lapsed, defaultSubscription) = await _unitOfWork.Run(
            /* The records themselves rather than a projection: each one is handed back to the writer as the
               current record it replaces, which no view of its values can stand in for. */
            x => x.MemberSiteSubscriptionRecordRepository
                .Query()
                .Current()
                .Expired(_siteSubscriptionCooldown)
                .ForPlatform(platform)
                .ForEnvironment(environment)
                .GetAll(),
            x => x.SiteSubscriptionRepository.GetDefaultOrDefault(environment, platform));

        /* A record already naming the plan it would be moved to is left alone, or the sweep appends one
           saying nothing new on every run. Settled before the plan is validated below, so a platform with
           nothing to downgrade reports nothing about a plan it does not need. */
        var toDowngrade = lapsed
            .Where(x => defaultSubscription == null || x.SiteSubscriptionId != defaultSubscription.Id)
            .ToArray();

        if (toDowngrade.Length == 0)
        {
            return;
        }

        if (defaultSubscription == null)
        {
            await _loggingService.Error(
                $"Cannot downgrade {toDowngrade.Length} lapsed site subscriptions: " +
                $"{platform} has no enabled default subscription in {environment}");
            return;
        }

        /* A downgrade takes no payment, so it can only ever land on a free plan - a member put on a priced
           one holds a plan they have not paid for, with nothing to renew. GetDefaultOrDefault asks only for
           enabled and default, so the plan can be priced without anything having said so. */
        if (!defaultSubscription.Free)
        {
            await _loggingService.Error(
                $"Cannot downgrade {toDowngrade.Length} lapsed site subscriptions: " +
                $"{platform}'s default subscription '{defaultSubscription.Name}' is not free");
            return;
        }

        var memberIds = toDowngrade
            .Select(x => x.MemberId)
            .ToArray();

        var (members, notificationSettings) = await _unitOfWork.Run(
            x => x.MemberRepository.GetByIds(memberIds),
            x => x.MemberNotificationSettingsRepository.GetByMemberIds(
                memberIds, NotificationType.SubscriptionDowngraded));

        var utcNow = DateTime.UtcNow;

        foreach (var record in toDowngrade)
        {
            /* Carries nothing of the plan it replaces: no expiry, because a free plan never expires and an
               inherited one would lapse the member again immediately, and no external id, price or payment,
               because those name a purchase this record is not. */
            _memberSiteSubscriptionWriter.MakeRecordCurrent(
                newRecord: new MemberSiteSubscriptionRecord
                {
                    CreatedUtc = utcNow,
                    MemberId = record.MemberId,
                    SiteSubscriptionId = defaultSubscription.Id
                },
                existingCurrent: record);

            if (!string.IsNullOrEmpty(record.ExternalId))
            {
                /* Past the expiry and the cooldown on top of it, the provider should have finished with this
                   subscription. One it has not is the case worth a human seeing. */
                await _loggingService.Info(
                    $"Downgrading member {record.MemberId} from a subscription still naming external " +
                    $"subscription '{record.ExternalId}'");
            }
        }

        _notificationService.AddSubscriptionDowngradedNotifications(
            defaultSubscription, members, notificationSettings);

        await _unitOfWork.SaveChanges();

        // After the commit: a member is never told about a downgrade that failed to save.
        foreach (var member in members)
        {
            await _memberEmailService.SendSiteSubscriptionExpiredEmail(request, member);
        }
    }

    public async Task<SiteSubscriptionsViewModel> GetSiteSubscriptionsViewModel(
        IServiceRequest request, Chapter? chapter)
    {
        var (environment, platform, memberId) =
            (request.Environment, request.Platform, request.CurrentMemberIdOrDefault);

        var (subscriptionDtos,
            prices,
            currentMember,
            memberSubscriptionDto,
            memberCurrency,
            chapterCurrency) = await _unitOfWork.Run(
            x => x.SiteSubscriptionRepository.Query()
                .ForPlatform(platform)
                .ForEnvironment(environment)
                .Active()
                .WithFeatures()
                .GetAll(),
            x => x.SiteSubscriptionPriceRepository.GetAllEnabled(platform),
            x => x.MemberRepository.GetByIdOrDefault(memberId),
            x => memberId != null
                ? x.MemberSiteSubscriptionRecordRepository.GetDtoByMemberId(memberId.Value)
                : DefaultDeferredQuerySingleOrDefault.For<MemberSiteSubscriptionDto>(),
            x => x.CurrencyRepository.GetByMemberIdOrDefault(memberId),
            x => x.CurrencyRepository.GetByChapterIdOrDefault(chapter?.Id));

        var currency = memberCurrency ?? chapterCurrency;

        var currencies = prices
            .Where(x => currency == null || x.CurrencyId == currency.Id)
            .GroupBy(x => x.CurrencyId)
            .Select(x => x.First().Currency)
            .ToArray();

        var priceDictionary = prices
            .GroupBy(x => x.SiteSubscriptionId)
            .ToDictionary(x => x.Key, x => (IReadOnlyCollection<SiteSubscriptionPrice>)x.ToArray());

        var externalSubscription = await GetExternalSubscription(memberSubscriptionDto);

        var siteSubscriptionViewModels = subscriptionDtos
            .Select(x => new
            {
                /* Every price the subscription has, which is what decides whether it is active - a paid plan
                   priced only in another currency is still active, it just has nothing to show this member.
                   The view model carries only the prices in the member's currency. */
                Prices = priceDictionary.GetValueOrDefault(x.SiteSubscription.Id, []),
                x.SiteSubscription
            })
            .Where(x => x.SiteSubscription.IsActive(x.Prices))
            .Select(x => new SiteSubscriptionListItemViewModel
            {
                Chapter = chapter,
                IsCurrentMemberActiveSubscription =
                    memberSubscriptionDto?.MemberSiteSubscription.SiteSubscriptionId == x.SiteSubscription.Id &&
                    externalSubscription?.Status == ExternalSubscriptionStatus.Active,
                Platform = platform,
                Prices = x.Prices
                    .Where(price => currency == null || price.CurrencyId == currency.Id)
                    .ToArray(),
                Subscription = x.SiteSubscription
            })
            .ToArray();

        return new SiteSubscriptionsViewModel
        {
            Chapter = chapter,
            Currencies = currencies,
            Currency = currency,
            CurrentMember = currentMember,
            CurrentMemberSubscription = memberSubscriptionDto,
            CurrentMemberExternalSubscription = externalSubscription,
            Subscriptions = siteSubscriptionViewModels
        };
    }

    public async Task<SiteSubscriptionCheckoutViewModel> StartSiteSubscriptionCheckout(
        IMemberServiceRequest request, Guid priceId, string returnPath)
    {
        var platform = request.Platform;

        var (subscription, price) = await _unitOfWork.Run(
            x => x.SiteSubscriptionRepository.GetByPriceId(priceId),
            x => x.SiteSubscriptionPriceRepository.GetById(priceId));

        /* The price id comes from the form, so it is checked against the platform selling it rather than
           trusted - the list it was chosen from is this platform's, and nothing else confines it. */
        OdkAssertions.MeetsCondition(subscription, x => x.Platform == platform);

        var (payment, externalCheckoutSession, publicApiKey) = await _paymentService.CreateSitePayment(
            request,
            subscription,
            price,
            new PaymentCreateOptions
            {
                ReturnPath = returnPath
            });

        return new SiteSubscriptionCheckoutViewModel
        {
            ApiPublicKey = publicApiKey,
            ClientSecret = externalCheckoutSession.ClientSecret,
            PaymentProvider = payment.PaymentProvider,
            SiteSubscription = subscription
        };
    }

    private async Task<ExternalSubscription?> GetExternalSubscription(
        MemberSiteSubscriptionDto? memberSubscriptionDto)
    {
        if (string.IsNullOrEmpty(memberSubscriptionDto?.MemberSiteSubscription?.ExternalId))
        {
            return null;
        }

        var siteSubscription = memberSubscriptionDto.SiteSubscription;
        var paymentProvider = _paymentProviderFactory.GetPaymentProvider(
            siteSubscription.PaymentProvider, siteSubscription.Platform);

        return await paymentProvider.GetSubscription(memberSubscriptionDto.MemberSiteSubscription.ExternalId);
    }
}