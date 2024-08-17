using ODK.Core;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Data.Core;

namespace ODK.Services.Subscriptions;

public class SiteSubscriptionAdminService : OdkAdminServiceBase, ISiteSubscriptionAdminService
{
    private readonly IPlatformProvider _platformProvider;
    private readonly IUnitOfWork _unitOfWork;

    public SiteSubscriptionAdminService(
        IUnitOfWork unitOfWork,
        IPlatformProvider platformProvider) 
        : base(unitOfWork)
    {
        _platformProvider = platformProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> AddSiteSubscription(Guid currentMemberId, SiteSubscriptionCreateModel model)
    {
        var existing = await GetAllSubscriptions(currentMemberId);

        var subscription = new SiteSubscription();
        subscription.Platform = _platformProvider.GetPlatform();
        UpdateSiteSubscription(model, subscription);

        _unitOfWork.SiteSubscriptionRepository.Add(subscription);

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> AddSiteSubscriptionPrice(Guid currentMemberId, Guid siteSubscriptionId,
        SiteSubscriptionPriceCreateModel model)
    {
        var (existing, currency) = await GetSuperAdminRestrictedContent(currentMemberId,
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

        _unitOfWork.SiteSubscriptionPriceRepository.Add(new SiteSubscriptionPrice
        {
            Amount = model.Amount,
            CurrencyId = model.CurrencyId,
            ExternalId = model.ExternalId,
            Frequency = model.Frequency,
            SiteSubscriptionId = siteSubscriptionId            
        });

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task DeleteSiteSubscriptionPrice(Guid currentMemberId, Guid siteSubscriptionId, Guid siteSubscriptionPriceId)
    {
        var price = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.SiteSubscriptionPriceRepository.GetById(siteSubscriptionPriceId));

        OdkAssertions.MeetsCondition(price, x => x.SiteSubscriptionId == siteSubscriptionId);

        _unitOfWork.SiteSubscriptionPriceRepository.Delete(price);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<SiteSubscription>> GetAllSubscriptions(Guid currentMemberId)
    {
        var platform = _platformProvider.GetPlatform();
        return await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.SiteSubscriptionRepository.GetAll(platform));
    }

    public async Task<SiteSubscriptionDto> GetSubscriptionDto(Guid currentMemberId, Guid siteSubscriptionId)
    {
        var (subscription, prices, currencies) = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.SiteSubscriptionRepository.GetById(siteSubscriptionId),
            x => x.SiteSubscriptionPriceRepository.GetBySiteSubscriptionId(siteSubscriptionId),
            x => x.CurrencyRepository.GetAll());

        return new SiteSubscriptionDto
        {
            Currencies = currencies,
            Prices = prices,
            Subscription = subscription
        };
    }

    public async Task<ServiceResult> UpdateSiteSubscription(Guid currentMemberId, Guid siteSubscriptionId, SiteSubscriptionCreateModel model)
    {
        var subscription = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.SiteSubscriptionRepository.GetById(siteSubscriptionId));

        UpdateSiteSubscription(model, subscription);

        _unitOfWork.SiteSubscriptionRepository.Update(subscription);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    private static void UpdateSiteSubscription(SiteSubscriptionCreateModel model, SiteSubscription subscription)
    {
        subscription.Description = model.Description;
        subscription.Enabled = model.Enabled;
        subscription.GroupLimit = model.GroupLimit;
        subscription.MemberLimit = model.MemberLimit;
        subscription.MemberSubscriptions = model.MemberSubscriptions;
        subscription.Name = model.Name;
        subscription.Premium = model.Premium;
        subscription.SendMemberEmails = model.SendMemberEmails;        
    }
}
