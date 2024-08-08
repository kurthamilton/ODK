using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ODK.Data.Core;

namespace ODK.Services.Subscriptions;

public class SiteSubscriptionService : ISiteSubscriptionService
{
    private readonly IUnitOfWork _unitOfWork;

    public SiteSubscriptionService(IUnitOfWork unitOfWork)
    { 
        _unitOfWork = unitOfWork;
    }

    public async Task<SiteSubscriptionsDto> GetSiteSubscriptionsDto()
    {
        var (subscriptions, prices) = await _unitOfWork.RunAsync(
            x => x.SiteSubscriptionRepository.GetAllEnabled(),
            x => x.SiteSubscriptionPriceRepository.GetAllEnabled());

        var currencies = prices
            .GroupBy(x => x.CurrencyId)
            .Select(x => x.First().Currency)
            .ToArray();

        var priceDictionary = prices
            .GroupBy(x => x.SiteSubscriptionId)
            .ToDictionary(x => x.Key, x => x.ToArray());

        return new SiteSubscriptionsDto
        {
            Currencies = currencies,
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
}
