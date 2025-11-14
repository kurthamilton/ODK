using System.Linq;
using ODK.Core.Extensions;
using ODK.Core.Payments;
using ODK.Core.Subscriptions;
using ODK.Core.Utils;
using ODK.Core.Web;
using ODK.Services.Payments;
using ODK.Services.Payments.Models;
using Stripe;
using Stripe.Checkout;

namespace ODK.Services.Integrations.Payments.Stripe;

public class StripePaymentProvider : IPaymentProvider
{
    private readonly IStripeClient _client;
    private readonly IHttpRequestProvider _httpRequestProvider;

    public StripePaymentProvider(
        IPaymentSettings paymentSettings,
        IHttpRequestProvider httpRequestProvider)
    {        
        _httpRequestProvider = httpRequestProvider;
        _client = new StripeClient(paymentSettings.ApiSecretKey);
    }

    public bool HasCustomers => true;

    public bool HasExternalGateway => true;

    public bool SupportsRecurringPayments => PaymentProviderType.Stripe.SupportsRecurringPayments();

    public async Task<ServiceResult> ActivateSubscriptionPlan(string externalId)
    {
        var service = CreatePriceService();

        await service.UpdateAsync(externalId, new PriceUpdateOptions
        {
            Active = true            
        });

        return ServiceResult.Successful();
    }

    public async Task<bool> CancelSubscription(string externalId)
    {
        var subscriptionService = CreateSubscriptionService();

        try
        {
            await subscriptionService.CancelAsync(externalId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public Task<string?> CreateCustomer(string emailAddress)
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
            Recurring = subscriptionPlan.Recurring ? new PriceRecurringOptions
            {
                Interval = subscriptionPlan.Frequency switch
                {
                    SiteSubscriptionFrequency.Monthly => "month",
                    SiteSubscriptionFrequency.Yearly => "year",
                    _ => ""
                },
                IntervalCount = subscriptionPlan.Frequency switch
                {
                    SiteSubscriptionFrequency.Monthly => subscriptionPlan.NumberOfMonths,
                    SiteSubscriptionFrequency.Yearly => subscriptionPlan.NumberOfMonths / 12,
                    _ => 1
                }
            } : null,
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

    public async Task<ExternalCheckoutSession?> GetCheckoutSession(string externalId)
    {
        var service = CreateSessionService();        

        try
        {
            var session = await service.GetAsync(externalId);

            var complete = session.Status == "complete";

            return new ExternalCheckoutSession
            {
                Amount = session.AmountTotal ?? 0,
                ClientSecret = session.ClientSecret,
                Complete = complete,
                Currency = session.Currency,
                SessionId = session.Id,
                SubscriptionId = session.SubscriptionId
            };
        }     
        catch
        {
            return null;
        }
    }

    public async Task<string?> GetProductId(string name)
    {
        var service = CreateProductService();
        var products = await service.ListAsync();
        return products
            .FirstOrDefault(x => string.Equals(name, x.Name, StringComparison.InvariantCultureIgnoreCase))
            ?.Id;
    }

    public async Task<IReadOnlyCollection<RemotePaymentModel>> GetAllPayments()
    {        
        var invoices = await GetAllInvoices(new InvoiceListOptions
        {
            Expand = ["data.payments"]
        });

        var remotePayments = new List<RemotePaymentModel>();

        foreach (var invoice in invoices)
        {
            var customer = invoice.Customer;

            foreach (var payment in invoice.Payments)
            {
                if (payment.AmountPaid == null)
                {
                    continue;
                }

                var remotePayment = new RemotePaymentModel
                {
                    Amount = (decimal)payment.AmountPaid / 100,
                    Created = invoice.Created,
                    Currency = invoice.Currency,
                    CustomerEmail = invoice.CustomerEmail,
                    Id = payment.Id,
                    SubscriptionId = invoice.Parent.SubscriptionDetails?.SubscriptionId
                };                

                remotePayments.Add(remotePayment);
            }
        }

        return remotePayments;
    }

    public async Task<ExternalSubscription?> GetSubscription(string externalId)
    {
        var service = CreateSubscriptionService();
        
        try
        {
            var subscription = await service.GetAsync(externalId);
            
            return new ExternalSubscription
            {
                CancelDate = subscription.CancelAt,
                ExternalId = subscription.Id,
                ExternalSubscriptionPlanId = "",
                // TODO: get last/next payment date
                LastPaymentDate = null,
                NextBillingDate = null,
                Status = subscription.Status == "active" && subscription.CancelAt == null
                    ? ExternalSubscriptionStatus.Active 
                    : ExternalSubscriptionStatus.Cancelled
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<ExternalSubscriptionPlan?> GetSubscriptionPlan(string externalId)
    {
        var service = CreatePriceService();

        try
        {
            var price = await service.GetAsync(externalId);

            var frequency = price.Recurring?.Interval switch
            {
                "month" => SiteSubscriptionFrequency.Monthly,
                "year" => SiteSubscriptionFrequency.Yearly,
                _ => SiteSubscriptionFrequency.None
            };

            var intervalCount = (int?)price.Recurring?.IntervalCount;

            return new ExternalSubscriptionPlan
            {
                Amount = price.UnitAmountDecimal ?? 0,
                CurrencyCode = price.Currency,
                ExternalId = price.Id,
                ExternalProductId = price.ProductId,
                Frequency = frequency,
                Name = price.Nickname,
                NumberOfMonths = frequency switch
                {
                    SiteSubscriptionFrequency.Yearly => intervalCount != null ? intervalCount.Value * 12 : 0,
                    _ => intervalCount ?? 0
                },
                Recurring = price.Recurring != null
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<RemotePaymentResult> MakePayment(string currencyCode, decimal amount, 
        string cardToken, string description, Guid memberId, string memberName)
    {
        var service = CreatePaymentIntentService();
        var intent = await service.CreateAsync(new PaymentIntentCreateOptions
        {
            Amount = (int)(amount * 100),
            Currency = currencyCode.ToLowerInvariant(),
            Description = $"{memberName}: {description}",
            ExtraParams = new Dictionary<string, object>
            {
                {
                    "payment_method_data", new Dictionary<string, object>
                    {
                        { "type", "card" },
                        {
                            "card", new Dictionary<string, object>
                            {
                                { "token", cardToken }
                            }
                        }
                    }

                }
            },            
            Metadata = new Dictionary<string, string>
            {
                { "member_id", memberId.ToString() }
            }
        });

        intent = await service.ConfirmAsync(intent.Id);
        return RemotePaymentResult.Successful(intent.Id);
    }

    public Task<string?> SendPayment(string currencyCode, decimal amount, 
        string emailAddress, string paymentId, string note)
    {
        throw new NotImplementedException();
    }

    public async Task<ExternalCheckoutSession> StartCheckout(ExternalSubscriptionPlan subscriptionPlan, string returnPath)
    {
        var baseUrl = UrlUtils.BaseUrl(_httpRequestProvider.RequestUrl);
        
        var service = CreateSessionService();
        var session = await service.CreateAsync(new SessionCreateOptions
        {
            UiMode = "embedded",
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Price = subscriptionPlan.ExternalId,
                    Quantity = 1
                }
            },
            Mode = subscriptionPlan.Recurring ? "subscription" : "payment",
            ReturnUrl = baseUrl + returnPath.Replace("{sessionId}", "{CHECKOUT_SESSION_ID}"),
            AutomaticTax = new SessionAutomaticTaxOptions { Enabled = false }
        });
        
        return new ExternalCheckoutSession
        {
            Amount = session.AmountTotal ?? 0,
            ClientSecret = session.ClientSecret,
            Complete = false,
            Currency = session.Currency,
            SessionId = session.Id,
            SubscriptionId = null
        };
    }

    public Task<ServiceResult> VerifyPayment(string currencyCode, decimal amount, string cardToken)
    {
        throw new NotImplementedException();
    }

    private async Task<IReadOnlyCollection<Invoice>> GetAllInvoices(
        InvoiceListOptions? options = null)
    {
        var service = CreateInvoiceService();

        return await service.ListAutoPagingAsync(options).All();
    }

    private InvoiceService CreateInvoiceService() => new InvoiceService(_client);
    private PaymentIntentService CreatePaymentIntentService() => new PaymentIntentService(_client);
    private PriceService CreatePriceService() => new PriceService(_client);
    private ProductService CreateProductService() => new ProductService(_client);
    private SessionService CreateSessionService() => new SessionService(_client);
    private SubscriptionService CreateSubscriptionService() => new SubscriptionService(_client);
}
