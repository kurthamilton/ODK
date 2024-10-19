using ODK.Core.Countries;
using ODK.Core.Payments;
using ODK.Core.Subscriptions;
using ODK.Services.Integrations.Payments.PayPal.Client;
using ODK.Services.Integrations.Payments.PayPal.Client.Models;
using ODK.Services.Payments;

namespace ODK.Services.Integrations.Payments.PayPal;

public class PayPalPaymentProvider : IPaymentProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPaymentSettings _paymentSettings;
    private readonly PayPalPaymentProviderSettings _settings;

    public PayPalPaymentProvider(
        PayPalPaymentProviderSettings settings,
        IHttpClientFactory httpClientFactory,
        IPaymentSettings paymentSettings)
    {
        _httpClientFactory = httpClientFactory;
        _paymentSettings = paymentSettings;
        _settings = settings;
    }

    public bool HasCustomers => false;

    public bool HasExternalGateway => false;    

    public async Task<ServiceResult> ActivateSubscriptionPlan(string externalId)
    {
        var client = GetClient();
        var success = await client.ActivateSubscriptionPlan(externalId);
        return success
            ? ServiceResult.Successful()
            : ServiceResult.Failure("Subscription plan could not be activated");
    }

    public async Task<bool> CancelSubscription(string externalId)
    {
        var client = GetClient();
        return await client.CancelSubscription(externalId);
    }

    public Task<string?> CreateCustomer(string emailAddress)
    {
        throw new NotImplementedException();
    }

    public async Task<string?> CreateProduct(string name)
    {
        var model = new ProductJsonModel
        {
            Name = name
        };

        var client = GetClient();
        var response = await client.CreateProduct(model);
        return response?.Id;
    }

    public async Task<string?> CreateSubscriptionPlan(ExternalSubscriptionPlan subscriptionPlan)
    {
        var model = new SubscriptionPlanJsonModel
        {
            BillingCycles =
            [
                new BillingCycleJsonModel
                {
                    Frequency = FrequencyJsonModel.ForFrequency(subscriptionPlan.Frequency),
                    PricingScheme = new PricingSchemeJsonModel
                    {
                        FixedPrice = new FixedPriceJsonModel
                        {
                            CurrencyCode = subscriptionPlan.CurrencyCode,
                            Value = Currency.ToValueString(subscriptionPlan.Amount)
                        }
                    },
                    Sequence = 1,
                    TenureType = "REGULAR",
                    TotalCycles = 0
                }
            ],
            Name = subscriptionPlan.Name,
            ProductId = subscriptionPlan.ExternalProductId,
            Status = "INACTIVE"
        };

        var client = GetClient();
        var response = await client.CreateSubscriptionPlan(model);
        return response?.Id;
    }

    public async Task<ServiceResult> DeactivateSubscriptionPlan(string externalId)
    {
        var client = GetClient();
        var success = await client.DeactivateSubscriptionPlan(externalId);
        return success
            ? ServiceResult.Successful()
            : ServiceResult.Failure("Subscription plan could not be deactivated");
    }

    public Task<string?> GetProductId(string name)
    {
        throw new NotImplementedException();
    }

    public async Task<ExternalSubscription?> GetSubscription(string externalId)
    {
        var client = GetClient();
        var subscription = await client.GetSubscription(externalId);
        return subscription != null ? new ExternalSubscription
        {
            ExternalId = subscription.Id,
            ExternalSubscriptionPlanId = subscription.PlanId,
            IsActive = subscription.Status == "ACTIVE",
            LastPaymentDate = subscription.BillingInfo?.LastPayment?.Date,
            NextBillingDate = subscription.BillingInfo?.NextBillingDate
        } : null;
    }

    public async Task<ExternalSubscriptionPlan?> GetSubscriptionPlan(string externalId)
    {
        var client = GetClient();
        var plan = await client.GetSubscriptionPlan(externalId);
        if (plan == null || plan.BillingCycles.Length != 1)
        {
            return null;
        }

        var billingCycle = plan.BillingCycles[0];
        var fixedPrice = billingCycle.PricingScheme.FixedPrice;
        if (fixedPrice == null)
        {
            return null;
        }

        decimal.TryParse(fixedPrice.Value, out var amount);

        return new ExternalSubscriptionPlan
        {
            Amount = amount,
            CurrencyCode = fixedPrice.CurrencyCode ?? "",
            ExternalId = externalId,
            ExternalProductId = plan.ProductId,
            Frequency = billingCycle.Frequency.OdkFrequency,
            Name = plan.Name,
            NumberOfMonths = billingCycle.Frequency.OdkFrequency == SiteSubscriptionFrequency.Yearly
                ? 12
                : 1
        };
    }

    public async Task<ServiceResult> MakePayment(string currencyCode, decimal amount,
        string cardToken, string description, string memberName)
    {
        var client = GetClient();

        var order = await client.GetOrder(cardToken);
        if (order == null || order.PurchaseUnits.Length != 1)
        {
            return ServiceResult.Failure("Payment not found in PayPal");
        }

        var purchase = order.PurchaseUnits[0];

        var approved = string.Equals(purchase.Amount?.CurrencyCode, currencyCode, StringComparison.InvariantCultureIgnoreCase) &&
            purchase.Amount?.Value == amount &&
            string.Equals("APPROVED", order.Status, StringComparison.InvariantCultureIgnoreCase);
        if (!approved)
        {
            return ServiceResult.Failure($"Payment not approved in PayPal. Current status: {order.Status}");
        }

        var capture = await client.CaptureOrderPayment(order.Id);
        if (!string.Equals("COMPLETED", capture?.Status, StringComparison.InvariantCultureIgnoreCase))
        {
            return ServiceResult.Failure($"Payment not completed in PayPal. Current status: {order.Status}");
        }

        return ServiceResult.Successful();
    }

    public async Task<string?> SendPayment(string currencyCode, decimal amount, string emailAddress,
        string paymentId, string note)
    {
        var client = GetClient();
        var response = await client.CreatePayout(new PayoutBatchJsonModel
        {
            Items =
            [
                new PayoutBatchItemJsonModel
                {
                    Amount = new PayoutAmountJsonModel
                    {
                        CurrencyCode = currencyCode,
                        Value = amount.ToString("0.00")
                    },
                    Note = note,
                    Receiver = emailAddress,
                    RecipientType = "EMAIL"
                }
            ],
            SenderBatchHeader = new PayoutBatchSenderHeaderJsonModel
            {
                SenderBatchId = paymentId
            }
        });

        return response?.BatchHeader?.PayoutBatchId;
    }

    public Task<string> StartCheckout(ExternalSubscriptionPlan subscriptionPlan)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult> VerifyPayment(string currencyCode, decimal amount, string cardToken)
    {
        throw new NotImplementedException();
    }

    private PayPalClient GetClient()
    {
        if (!_paymentSettings.HasApiKey)
        {
            throw new Exception("PayPal API settings missing");
        }

        return new PayPalClient(
            _settings.ApiBaseUrl,
            _paymentSettings.ApiPublicKey ?? "",
            _paymentSettings.ApiSecretKey ?? "",
            _httpClientFactory);
    }
}
