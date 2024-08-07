using ODK.Core.Subscriptions;
using ODK.Data.Core;

namespace ODK.Services.Subscriptions;

public class SiteSubscriptionAdminService : OdkAdminServiceBase, ISiteSubscriptionAdminService
{
    private readonly IUnitOfWork _unitOfWork;

    public SiteSubscriptionAdminService(IUnitOfWork unitOfWork) 
        : base(unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> AddSiteSubscription(Guid currentMemberId, SiteSubscriptionCreateModel model)
    {
        var existing = await GetAllSubscriptions(currentMemberId);

        var subscription = new SiteSubscription
        {
            Description = model.Description,
            Enabled = model.Enabled,
            GroupLimit = model.GroupLimit,
            MemberLimit = model.MemberLimit,
            MemberSubscriptions = model.MemberSubscriptions,
            Name = model.Name,
            Premium = model.Premium,
            SendMemberEmails = model.SendMemberEmails
        };

        _unitOfWork.SiteSubscriptionRepository.Add(subscription);

        foreach (var price in model.Prices)
        {
            _unitOfWork.SiteSubscriptionPriceRepository.Add(new SiteSubscriptionPrice
            {
                Amount = price.Amount,
                CurrencyCode = price.CurrencyCode,
                CurrencySymbol = price.CurrencySymbol,
                Months = price.Months,
                SiteSubscriptionId = subscription.Id
            });
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<IReadOnlyCollection<SiteSubscription>> GetAllSubscriptions(Guid currentMemberId)
    {
        return await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.SiteSubscriptionRepository.GetAll());
    }
}
