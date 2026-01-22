using ODK.Core.Extensions;
using ODK.Core.Payments;
using ODK.Core.Subscriptions;
using ODK.Core.Utils;
using ODK.Services.Logging;
using ODK.Services.Payments;
using ODK.Services.Payments.Models;
using Stripe;
using Stripe.Checkout;

namespace ODK.Services.Integrations.Payments.Stripe;

public class StripePaymentProvider : IPaymentProvider
{
    private readonly IStripeClient _client;
    private readonly string? _connectedAccountId;
    private readonly ILoggingService _loggingService;
    private readonly StripePaymentProviderSettings _settings;

    public StripePaymentProvider(
        SitePaymentSettings paymentSettings,
        ILoggingService loggingService,
        string? connectedAccountId,
        StripePaymentProviderSettings settings)
    {
        _client = new StripeClient(new StripeClientOptions
        {
            ApiKey = paymentSettings.ApiSecretKey
        });
        _connectedAccountId = connectedAccountId;
        _loggingService = loggingService;
        _settings = settings;
    }

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
        await _loggingService.Info($"Cancelling Stripe subscription '{externalId}'");

        var service = CreateSubscriptionService();

        try
        {
            await service.CancelAsync(externalId);
            return true;
        }
        catch (Exception ex)
        {
            await _loggingService.Error($"Error cancelling Stripe subscription '{externalId}'", ex);
            return false;
        }
    }

    public async Task<RemoteAccount?> CreateConnectedAccount(CreateRemoteAccountOptions options)
    {
        var emailAddress = options.Owner.EmailAddress;

        await _loggingService.Info($"Creating connected stripe account for '{emailAddress}'");

        var service = CreateAccountService();

        try
        {
            var account = await service.CreateAsync(new AccountCreateOptions
            {
                Email = emailAddress,
                Country = options.Country.IsoCode2,
                Type = "express",
                BusinessProfile = new AccountBusinessProfileOptions
                {
                    Name = options.Chapter.FullName,
                    Url = CleanConnectedAccountUrl(options.ChapterUrl),
                    Mcc = _settings.ConnectedAccountMcc,
                    ProductDescription = _settings.ConnectedAccountProductDescription
                },
                BusinessType = "individual",
                DefaultCurrency = options.ChapterCurrency.Code,
                Individual = new AccountIndividualOptions
                {
                    Email = options.Owner.EmailAddress,
                    FirstName = options.Owner.FirstName,
                    LastName = options.Owner.LastName
                },
                Capabilities = new AccountCapabilitiesOptions
                {
                    CardPayments = new AccountCapabilitiesCardPaymentsOptions
                    {
                        Requested = true
                    },
                    Transfers = new AccountCapabilitiesTransfersOptions
                    {
                        Requested = true
                    }
                }
            });

            return new RemoteAccount
            {
                CardPaymentsEnabled = false,
                Id = account.Id,
                IdentityDocumentsProvided = false,
                InitialOnboardingComplete = false,
            };
        }
        catch (Exception ex)
        {
            await _loggingService.Error($"Error creating connected stripe account for '{emailAddress}'", ex);
            return null;
        }
    }

    public async Task<string?> CreateProduct(string name)
    {
        var service = CreateProductService();
        var result = await service.SearchAsync(new ProductSearchOptions
        {
            Query = $"name:\"{name}\""
        });

        var match = result
            .FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

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

        var options = new PriceCreateOptions
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
                    _ => string.Empty
                },
                IntervalCount = subscriptionPlan.Frequency switch
                {
                    SiteSubscriptionFrequency.Monthly => subscriptionPlan.NumberOfMonths,
                    SiteSubscriptionFrequency.Yearly => subscriptionPlan.NumberOfMonths / 12,
                    _ => 1
                }
            } : null,
            UnitAmount = ToStripeAmount(subscriptionPlan.Amount)
        };

        var result = await service.CreateAsync(options);

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

    public async Task<string?> GenerateConnectedAccountSetupUrl(GenerateRemoteAccountSetupUrlOptions options)
    {
        await _loggingService.Info($"Refreshing connected stripe account for Stripe account '{options.Id}'");

        var service = CreateAccountLinkService();

        try
        {
            var link = service.Create(new AccountLinkCreateOptions
            {
                Account = options.Id,
                Type = "account_onboarding",
                RefreshUrl = options.RefreshUrl,
                ReturnUrl = options.ReturnUrl
            });

            return link.Url;
        }
        catch (Exception ex)
        {
            await _loggingService.Error($"Error refreshing connected stripe account for Stripe account '{options.Id}'", ex);
            return null;
        }
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
                Metadata = PaymentMetadataModel.FromDictionary(session.Metadata ?? []),
                PaymentId = session.PaymentIntentId,
                SessionId = session.Id,
                SubscriptionId = session.SubscriptionId
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<RemoteAccount?> GetConnectedAccount(string externalId)
    {
        var service = CreateAccountService();

        var account = await service.GetAsync(externalId);

        var initialOnboardingComplete = account.PayoutsEnabled;
        var identityDocumentsProvided = initialOnboardingComplete &&
            !account.Requirements?.EventuallyDue?.Contains("individual.verification.document") == true;

        return new RemoteAccount
        {
            CardPaymentsEnabled = account.Capabilities.CardPayments == "active",
            Id = account.Id,
            IdentityDocumentsProvided = identityDocumentsProvided,
            InitialOnboardingComplete = initialOnboardingComplete
        };
    }

    public async Task<string?> GetProductId(string name)
    {
        var service = CreateProductService();
        var products = await service.ListAsync();
        return products
            .FirstOrDefault(x => string.Equals(name, x.Name, StringComparison.InvariantCultureIgnoreCase))
            ?.Id;
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
                ConnectedAccountId = subscription.TransferData?.DestinationId,
                ExternalId = subscription.Id,
                ExternalSubscriptionPlanId = string.Empty,
                // TODO: get last/next payment date
                LastPaymentDate = null,
                Metadata = subscription.Metadata,
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
                Amount = FromStripeAmount(price.UnitAmount),
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

    public async Task<RemotePaymentResult> MakePayment(
        string currencyCode,
        decimal amount,
        string cardToken,
        string description,
        Guid memberId,
        string memberName)
    {
        var service = CreatePaymentIntentService();

        var stripeAmount = ToStripeAmount(amount);
        var intent = await service.CreateAsync(new PaymentIntentCreateOptions
        {
            Amount = stripeAmount,
            ApplicationFeeAmount = CalculateCommission(stripeAmount),
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
            },
            TransferData = !string.IsNullOrEmpty(_connectedAccountId)
                ? new PaymentIntentTransferDataOptions { Destination = _connectedAccountId }
                : null,
        });

        intent = await service.ConfirmAsync(intent.Id);
        return RemotePaymentResult.Successful(intent.Id);
    }

    public async Task<ExternalCheckoutSession> StartCheckout(
        ServiceRequest request,
        string emailAddress,
        ExternalSubscriptionPlan subscriptionPlan,
        string returnPath,
        PaymentMetadataModel metadata)
    {
        var returnUrl = UrlUtils.Url(
            baseUrl: request.HttpRequestContext.BaseUrl,
            path: returnPath.Replace("{sessionId}", "{CHECKOUT_SESSION_ID}"));

        var metadataDictionary = new Dictionary<string, string>(metadata.ToDictionary());

        var service = CreateSessionService();

        var stripeAmount = ToStripeAmount(subscriptionPlan.Amount);

        var isPrice = !string.IsNullOrEmpty(subscriptionPlan.ExternalId);

        var session = await service.CreateAsync(new SessionCreateOptions
        {
            UiMode = "embedded",
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Price = isPrice ? subscriptionPlan.ExternalId : null,
                    PriceData = !isPrice ? new SessionLineItemPriceDataOptions
                    {
                        Currency = subscriptionPlan.CurrencyCode,
                        Product = subscriptionPlan.ExternalProductId,
                        UnitAmount = stripeAmount
                    } : null,
                    Quantity = 1
                }
            },
            Mode = subscriptionPlan.Recurring ? "subscription" : "payment",
            ReturnUrl = returnUrl,
            AutomaticTax = new SessionAutomaticTaxOptions { Enabled = false },
            Metadata = metadataDictionary,
            CustomerEmail = emailAddress,
            PaymentIntentData = !subscriptionPlan.Recurring ? new SessionPaymentIntentDataOptions
            {
                ApplicationFeeAmount = CalculateCommission(stripeAmount),
                Metadata = metadataDictionary,
                TransferData = !string.IsNullOrEmpty(_connectedAccountId)
                    ? new SessionPaymentIntentDataTransferDataOptions { Destination = _connectedAccountId }
                    : null
            } : null,
            SubscriptionData = subscriptionPlan.Recurring ? new SessionSubscriptionDataOptions
            {
                ApplicationFeePercent = !string.IsNullOrEmpty(_connectedAccountId)
                    ? _settings.ConnectedAccountCommissionPercentage
                    : null,
                Metadata = metadataDictionary,
                TransferData = !string.IsNullOrEmpty(_connectedAccountId)
                    ? new SessionSubscriptionDataTransferDataOptions { Destination = _connectedAccountId }
                    : null
            } : null
        });

        return new ExternalCheckoutSession
        {
            Amount = session.AmountTotal ?? 0,
            ClientSecret = session.ClientSecret,
            Complete = false,
            Currency = session.Currency,
            Metadata = metadata,
            PaymentId = null,
            SessionId = session.Id,
            SubscriptionId = null
        };
    }

    private static decimal FromStripeAmount(long? stripeAmount) => (stripeAmount ?? 0) / 100;

    private static long ToStripeAmount(decimal amount) => (long)(amount * 100);

    private long? CalculateCommission(long stripeAmount)
    {
        if (string.IsNullOrEmpty(_connectedAccountId))
        {
            return null;
        }

        return (long)(stripeAmount * _settings.ConnectedAccountCommissionPercentage / 100);
    }

    private string? CleanConnectedAccountUrl(string url)
    {
        // do not send localhost to Stripe
        var baseUrl = UrlUtils.BaseUrl(url);
        if (!string.IsNullOrEmpty(_settings.ConnectedAccountBaseUrl))
        {
            return url.Replace(baseUrl, _settings.ConnectedAccountBaseUrl, StringComparison.OrdinalIgnoreCase);
        }

        return baseUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase)
            ? baseUrl
            : null;
    }

    private AccountLinkService CreateAccountLinkService() => new(_client);

    private AccountService CreateAccountService() => new(_client);

    private PaymentIntentService CreatePaymentIntentService() => new(_client);

    private PriceService CreatePriceService() => new(_client);

    private ProductService CreateProductService() => new(_client);

    private SessionService CreateSessionService() => new(_client);

    private SubscriptionService CreateSubscriptionService() => new(_client);
}