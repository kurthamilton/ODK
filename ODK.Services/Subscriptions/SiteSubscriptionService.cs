using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ODK.Data.Core;
using ODK.Services.Platforms;

namespace ODK.Services.Subscriptions;

public class SiteSubscriptionService : ISiteSubscriptionService
{
    private readonly IPlatformProvider _platformProvider;
    private readonly IUnitOfWork _unitOfWork;

    public SiteSubscriptionService(IUnitOfWork unitOfWork, IPlatformProvider platformProvider)
    { 
        _platformProvider = platformProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<SiteSubscriptionsDto> GetSiteSubscriptionsDto()
    {
        var platform = _platformProvider.GetPlatform();
        var (subscriptions, prices) = await _unitOfWork.RunAsync(
            x => x.SiteSubscriptionRepository.GetAllEnabled(platform),
            x => x.SiteSubscriptionPriceRepository.GetAllEnabled(platform));

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
