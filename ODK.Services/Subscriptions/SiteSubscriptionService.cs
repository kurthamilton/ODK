using ODK.Core;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Subscriptions;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Members;
using ODK.Services.Payments;
using ODK.Services.Payments.Models;
using ODK.Services.Subscriptions.ViewModels;

namespace ODK.Services.Subscriptions;

public class SiteSubscriptionService : ISiteSubscriptionService
{
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly IPaymentService _paymentService;
    private readonly IUnitOfWork _unitOfWork;

    public SiteSubscriptionService(
        IUnitOfWork unitOfWork,
        IPaymentProviderFactory paymentProviderFactory,
        IPaymentService paymentService)
    {
        _paymentProviderFactory = paymentProviderFactory;
        _paymentService = paymentService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> CancelMemberSiteSubscription(IMemberServiceRequest request)
    {
        var currentMember = request.CurrentMember;

        var memberSubscriptionDto = await _unitOfWork.Run(
            x => x.MemberSiteSubscriptionRecordRepository.GetDtoByMemberId(currentMember.Id));

        if (memberSubscriptionDto == null)
        {
            return ServiceResult.Failure("Subscription not found");
        }

        OdkAssertions.MeetsCondition(memberSubscriptionDto.MemberSiteSubscription, x => x.MemberId == currentMember.Id);

        if (string.IsNullOrEmpty(memberSubscriptionDto.MemberSiteSubscription.ExternalId))
        {
            return ServiceResult.Failure("External subscription not found");
        }

        var siteSubscription = memberSubscriptionDto.SiteSubscription;
        var paymentProvider = _paymentProviderFactory.GetPaymentProvider(
            siteSubscription.PaymentProvider, siteSubscription.Platform);

        var result = await paymentProvider.CancelSubscription(memberSubscriptionDto.MemberSiteSubscription.ExternalId);

        return result
            ? ServiceResult.Successful()
            : ServiceResult.Failure("Subscription could not be cancelled");
    }

    public async Task<SiteSubscriptionsViewModel> GetSiteSubscriptionsViewModel(
        IServiceRequest request, Guid? chapterId)
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
            x => memberId != null
                ? x.MemberRepository.GetByIdOrDefault(memberId.Value)
                : new DefaultDeferredQuerySingleOrDefault<Member>(),
            x => memberId != null
                ? x.MemberSiteSubscriptionRecordRepository.GetDtoByMemberId(memberId.Value)
                : new DefaultDeferredQuerySingleOrDefault<MemberSiteSubscriptionDto>(),
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
                IsCurrentMemberActiveSubscription =
                    memberSubscriptionDto?.MemberSiteSubscription.SiteSubscriptionId == x.SiteSubscription.Id &&
                    externalSubscription?.Status == ExternalSubscriptionStatus.Active,
                Prices = x.Prices
                    .Where(price => currency == null || price.CurrencyId == currency.Id)
                    .ToArray(),
                Subscription = x.SiteSubscription
            })
            .ToArray();

        return new SiteSubscriptionsViewModel
        {
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
        var (platform, currentMember) = (request.Platform, request.CurrentMember);

        var (subscription, price) = await _unitOfWork.Run(
            x => x.SiteSubscriptionRepository.GetByPriceId(priceId),
            x => x.SiteSubscriptionPriceRepository.GetById(priceId));

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