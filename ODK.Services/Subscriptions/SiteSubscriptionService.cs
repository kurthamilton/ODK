using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Payments;

namespace ODK.Services.Subscriptions;

public class SiteSubscriptionService : ISiteSubscriptionService
{
    private readonly IPaymentService _paymentService;
    private readonly IPlatformProvider _platformProvider;
    private readonly IUnitOfWork _unitOfWork;

    public SiteSubscriptionService(
        IUnitOfWork unitOfWork, 
        IPlatformProvider platformProvider,
        IPaymentService paymentService)
    { 
        _paymentService = paymentService;
        _platformProvider = platformProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<MemberSiteSubscription?> GetMemberSiteSubscription(Guid memberId)
    {
        return await _unitOfWork.MemberSiteSubscriptionRepository.GetByMemberId(memberId).RunAsync();
    }

    public async Task<SiteSubscriptionsDto> GetSiteSubscriptionsDto(Guid? memberId, Guid? chapterId)
    {
        var platform = _platformProvider.GetPlatform();
        var (chapter, subscriptions, prices, memberPaymentSettings, chapterPaymentSettings) = await _unitOfWork.RunAsync(
            x => chapterId != null
                ? x.ChapterRepository.GetByIdOrDefault(chapterId.Value)
                : new DefaultDeferredQuerySingleOrDefault<Chapter>(),
            x => x.SiteSubscriptionRepository.GetAllEnabled(platform),
            x => x.SiteSubscriptionPriceRepository.GetAllEnabled(platform),
            x => memberId != null 
                ? x.MemberPaymentSettingsRepository.GetByMemberId(memberId.Value)
                : new DefaultDeferredQuerySingleOrDefault<MemberPaymentSettings>(),
            x => chapterId != null 
                ? x.ChapterPaymentSettingsRepository.GetByChapterId(chapterId.Value)
                : new DefaultDeferredQuerySingleOrDefault<ChapterPaymentSettings>());        

        var currency = chapterPaymentSettings?.Currency ?? memberPaymentSettings?.Currency;

        var currencies = prices
            .Where(x => currency == null || x.CurrencyId == currency.Id)
            .GroupBy(x => x.CurrencyId)
            .Select(x => x.First().Currency)
            .ToArray();

        var priceDictionary = prices
            .Where(x => currency == null || x.CurrencyId == currency.Id)
            .GroupBy(x => x.SiteSubscriptionId)
            .ToDictionary(x => x.Key, x => x.ToArray());        

        return new SiteSubscriptionsDto
        {
            Chapter = chapter,
            Currencies = currencies,            
            Currency = currency,
            PaymentProvider = chapterPaymentSettings?.Provider ?? memberPaymentSettings?.Provider,
            Subscriptions = subscriptions
                .Where(x => priceDictionary.ContainsKey(x.Id))
                .Select(x => new SiteSubscriptionDto
                {
                    Currencies = [],
                    Prices = priceDictionary[x.Id],
                    Subscription = x
                })
                .ToArray()
        };
    }

    public async Task<ServiceResult> UpdateMemberSiteSubscription(Guid memberId, Guid siteSubscriptionId,
        SiteSubscriptionFrequency frequency)
    {
        var (memberPaymentSettings, siteSubscription, memberSubscription) = await _unitOfWork.RunAsync(
                x => x.MemberPaymentSettingsRepository.GetByMemberId(memberId),
                x => x.SiteSubscriptionPriceRepository.GetBySiteSubscriptionId(siteSubscriptionId),
                x => x.MemberSiteSubscriptionRepository.GetByMemberId(memberId));

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
        var chapters = await _unitOfWork.ChapterRepository.GetByOwnerId(ownerId).RunAsync();
        if (chapters.Count == 0)
        {
            return null;
        }

        foreach (var chapter in chapters)
        {
            var chapterPaymentSettings = await _unitOfWork.ChapterPaymentSettingsRepository
                .GetByChapterId(chapter.Id)
                .RunAsync();

            if (chapterPaymentSettings != null)
            {
                return chapterPaymentSettings;
            }
        }

        return null;
    }
}
