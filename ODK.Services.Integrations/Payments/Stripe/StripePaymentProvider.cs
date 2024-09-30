using ODK.Core.Payments;
using ODK.Core.Subscriptions;
using ODK.Services.Payments;
using Stripe;

namespace ODK.Services.Integrations.Payments.Stripe;

public class StripePaymentProvider : IPaymentProvider
{
    private readonly IStripeClient _client;
    private readonly IPaymentSettings _paymentSettings;
    private readonly StripePaymentProviderSettings _settings;

    public StripePaymentProvider(
        StripePaymentProviderSettings settings,
        IPaymentSettings paymentSettings)
    {        
        _paymentSettings = paymentSettings;
        _settings = settings;
        _client = new StripeClient(paymentSettings.ApiSecretKey);
    }

    public bool HasCustomers => true;

    public bool HasExternalGateway => true;

    public async Task<ServiceResult> ActivateSubscriptionPlan(string externalId)
    {
        var service = CreatePriceService();

        await service.UpdateAsync(externalId, new PriceUpdateOptions
        {
            Active = true            
        });

        return ServiceResult.Successful();
    }

    public Task<bool> CancelSubscription(string externalId)
    {
        throw new NotImplementedException();
    }

    public Task<string> CreateCustomer(string emailAddress)
    {
        throw new NotImplementedException();
    }

    public async Task<string?> CreateProduct(string name)
    {
        var service = CreateProductService();
        var result = await service.SearchAsync(new ProductSearchOptions
        {
            Query = $"name:\"{name}\""
        });

        var match = result
            .FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase));

        if (match != null)
        {
            return match.Id;
        }

        var product = await service.CreateAsync(new ProductCreateOptions
        {
            Name = name
        });

        return product.Id;
    }

    public async Task<string?> CreateSubscriptionPlan(ExternalSubscriptionPlan subscriptionPlan)
    {
        var service = CreatePriceService();
        
        var result = await service.CreateAsync(new PriceCreateOptions
        {
            Active = false,
            Currency = subscriptionPlan.CurrencyCode.ToLowerInvariant(),
            Nickname = subscriptionPlan.Name,
            Product = subscriptionPlan.ExternalProductId,
            Recurring = new PriceRecurringOptions
            {
                Interval = subscriptionPlan.Frequency switch
                {
                    SiteSubscriptionFrequency.Monthly => "month",
                    SiteSubscriptionFrequency.Yearly => "year",
                    _ => ""
                },
                IntervalCount = 1
            },
            UnitAmountDecimal = subscriptionPlan.Amount * 100
        });
        
        return result.Id;
    }

    public async Task<ServiceResult> DeactivateSubscriptionPlan(string externalId)
    {
        var service = CreatePriceService();
        
        await service.UpdateAsync(externalId, new PriceUpdateOptions
        {
            Active = false            
        });

        return ServiceResult.Successful();
    }

    public Task<ExternalSubscription?> GetSubscription(string externalId)
    {
        throw new NotImplementedException();
    }

    public async Task<ExternalSubscriptionPlan?> GetSubscriptionPlan(string externalId)
    {
        var service = CreatePriceService();

        try
        {
            var price = await service.GetAsync(externalId);

            return new ExternalSubscriptionPlan
            {
                Amount = price.UnitAmountDecimal ?? 0,
                CurrencyCode = price.Currency,
                ExternalId = price.Id,
                ExternalProductId = price.ProductId,
                Frequency = price.Recurring.Interval switch
                {
                    "month" => SiteSubscriptionFrequency.Monthly,
                    "year" => SiteSubscriptionFrequency.Yearly,
                    _ => SiteSubscriptionFrequency.None
                },
                Name = price.Nickname
            };
        }     
        catch
        {
            return null;
        }
    }

    public Task<ServiceResult> MakePayment(string currencyCode, decimal amount, string cardToken, string description, string memberName)
    {
        throw new NotImplementedException();
    }

    public Task<string?> SendPayment(string currencyCode, decimal amount, string emailAddress, string paymentId, string note)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult> VerifyPayment(string currencyCode, decimal amount, string cardToken)
    {
        throw new NotImplementedException();
    }

    private CustomerService CreateCustomerService() => new CustomerService(_client);

    private PriceService CreatePriceService() => new PriceService(_client);

    private ProductService CreateProductService() => new ProductService(_client);

    private SubscriptionService CreateSubscriptionService() => new SubscriptionService(_client);
}
