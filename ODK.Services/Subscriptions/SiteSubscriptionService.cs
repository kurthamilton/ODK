using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Members;
using ODK.Services.Payments;

namespace ODK.Services.Subscriptions;

public class SiteSubscriptionService : ISiteSubscriptionService
{
    private readonly IMemberEmailService _memberEmailService;
    private readonly IPaymentService _paymentService;
    private readonly IPlatformProvider _platformProvider;
    private readonly IUnitOfWork _unitOfWork;

    public SiteSubscriptionService(
        IUnitOfWork unitOfWork, 
        IPlatformProvider platformProvider,
        IPaymentService paymentService,
        IMemberEmailService memberEmailService)
    { 
        _memberEmailService = memberEmailService;
        _paymentService = paymentService;
        _platformProvider = platformProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> ConfirmMemberSiteSubscription(
        Guid memberId, 
        Guid siteSubscriptionPriceId, 
        string externalId)
    {
        var platform = _platformProvider.GetPlatform();

        var (paymentSettings, memberSubscription, siteSubscriptionPrice) = await _unitOfWork.RunAsync(
            x => x.SitePaymentSettingsRepository.Get(),
            x => x.MemberSiteSubscriptionRepository.GetByMemberId(memberId, platform),
            x => x.SiteSubscriptionPriceRepository.GetById(siteSubscriptionPriceId));

        var externalSubscription = await _paymentService.GetSubscription(paymentSettings, externalId);

        if (externalSubscription == null || 
            externalSubscription.ExternalSubscriptionPlanId != siteSubscriptionPrice.ExternalId)
        {
            return ServiceResult.Failure("Error confirming subscription");
        }        

        if (memberSubscription == null)
        {
            memberSubscription = new MemberSiteSubscription();
        }        
        
        memberSubscription.ExternalId = externalSubscription.ExternalId;
        memberSubscription.ExpiresUtc = externalSubscription.NextBillingDate;

        if (memberSubscription.SiteSubscriptionId != siteSubscriptionPrice.SiteSubscriptionId)
        {
            var siteSubscription = await _unitOfWork.SiteSubscriptionRepository
                .GetById(siteSubscriptionPrice.SiteSubscriptionId)
                .Run();
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

    public async Task<SiteSubscriptionsDto> GetSiteSubscriptionsDto(Guid? memberId)
    {
        var platform = _platformProvider.GetPlatform();
        var (sitePaymentSettings, subscriptions, prices, currentMember, memberPaymentSettings, memberSubscription) = await _unitOfWork.RunAsync(
            x => x.SitePaymentSettingsRepository.Get(),
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

        return new SiteSubscriptionsDto
        {
            Currencies = currencies,            
            Currency = currency,
            CurrentMember = currentMember,
            CurrentMemberSubscription = memberSubscription,
            PaymentSettings = sitePaymentSettings,
            Subscriptions = subscriptions
                .Select(x => new SiteSubscriptionDto
                {
                    Currencies = [],
                    Prices = priceDictionary.ContainsKey(x.Id) ? priceDictionary[x.Id] : [],
                    Subscription = x
                })
                .ToArray()
        };
    }

    public async Task SyncExpiredSubscriptions()
    {
        var (paymentSettings, subscriptions) = await _unitOfWork.RunAsync(
            x => x.SitePaymentSettingsRepository.Get(),
            x => x.MemberSiteSubscriptionRepository.GetExpired());

        var updated = new List<MemberSiteSubscription>();

        foreach (var subscription in subscriptions)
        {
            if (string.IsNullOrEmpty(subscription.ExternalId))
            {
                continue;
            }

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
