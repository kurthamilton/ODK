using ODK.Core.Chapters;
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
    private readonly ILoggingService _loggingService;
    private readonly StripePaymentProviderSettings _settings;
    
    public StripePaymentProvider(
        IPaymentSettings paymentSettings,
        ILoggingService loggingService,
        string? connectedAccountId,
        StripePaymentProviderSettings settings)
    {
        _client = new StripeClient(new StripeClientOptions
        {
            ApiKey = paymentSettings.ApiSecretKey,
            StripeAccount = connectedAccountId
        });
        _loggingService = loggingService;
        _settings = settings;
    }

    public bool HasCustomers => true;

    public bool HasExternalGateway => true;

    public bool SupportsConnectedAccounts => true;

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

        // do not send localhost to Stripe
        var chapterUrl = options.ChapterUrl;
        var baseUrl = UrlUtils.BaseUrl(chapterUrl);
        if (!string.IsNullOrEmpty(_settings.ConnectedAccountBaseUrl))
        {
            chapterUrl = chapterUrl.Replace(
                baseUrl, _settings.ConnectedAccountBaseUrl, StringComparison.OrdinalIgnoreCase);
        }

        baseUrl = UrlUtils.BaseUrl(chapterUrl);
        if (baseUrl.Contains("localhost"))
        {
            chapterUrl = null;
        }

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
                    Url = chapterUrl,
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

    public async Task<IReadOnlyCollection<RemotePaymentModel>> GetAllPayments()
    {
        var (invoices, paymentIntents, subscriptions) = await TaskUtils.WhenAll(
            GetAllInvoices(new InvoiceListOptions
            {
                Expand = ["data.payments"]
            }),
            GetAllPaymentIntents(new PaymentIntentListOptions
            {
                Expand = ["data.latest_charge"]
            }), 
            GetAllSubscriptions());

        var paymentIntentDictionary = paymentIntents
            .ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);

        var subscriptionDictionary = subscriptions
            .ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);

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

                var paymentIntentId = payment.Payment.PaymentIntentId;

                if (paymentIntentDictionary.TryGetValue(paymentIntentId, out var paymentIntent))
                {
                    if (paymentIntent.LatestCharge.Refunded)
                    {
                        continue;
                    }
                }

                var subscriptionId = invoice.Parent.SubscriptionDetails?.SubscriptionId;                

                if (!string.IsNullOrEmpty(subscriptionId))
                {
                    subscriptionDictionary.TryGetValue(subscriptionId, out var subscription);
                }

                var created = paymentIntent?.LatestCharge.Created ?? invoice.Created;

                var remotePayment = new RemotePaymentModel
                {
                    Amount = (decimal)payment.AmountPaid / 100,
                    Created = TimeZoneInfo.ConvertTimeFromUtc(created, Chapter.DefaultTimeZone),
                    Currency = invoice.Currency,
                    CustomerEmail = invoice.CustomerEmail,
                    PaymentId = paymentIntentId,
                    SubscriptionId = invoice.Parent.SubscriptionDetails?.SubscriptionId
                };                

                remotePayments.Add(remotePayment);
            }
        }

        return remotePayments;
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
            ReturnUrl = returnUrl,
            AutomaticTax = new SessionAutomaticTaxOptions { Enabled = false },
            Metadata = metadataDictionary,
            CustomerEmail = emailAddress,
            PaymentIntentData = !subscriptionPlan.Recurring ? new SessionPaymentIntentDataOptions
            {
                Metadata = metadataDictionary
            } : null,
            SubscriptionData = subscriptionPlan.Recurring ? new SessionSubscriptionDataOptions
            {
                Metadata = metadataDictionary
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

    public async Task UpdatePaymentMetadata(string externalId, PaymentMetadataModel metadata)
    {
        var service = CreatePaymentIntentService();

        await service.UpdateAsync(externalId, new PaymentIntentUpdateOptions
        {
            Metadata = new Dictionary<string, string>(metadata.ToDictionary())
        });
    }

    private async Task<IReadOnlyCollection<Invoice>> GetAllInvoices(
        InvoiceListOptions? options = null)
    {
        var service = CreateInvoiceService();

        return await service.ListAutoPagingAsync(options).All();
    }

    private async Task<IReadOnlyCollection<PaymentIntent>> GetAllPaymentIntents(
        PaymentIntentListOptions? options = null)
    {
        var service = CreatePaymentIntentService();        
        return await service.ListAutoPagingAsync(options).All();
    }

    private async Task<IReadOnlyCollection<Subscription>> GetAllSubscriptions()
    {
        var service = CreateSubscriptionService();
        return await service.ListAutoPagingAsync().All();
    }

    private AccountLinkService CreateAccountLinkService() => new(_client);
    private AccountService CreateAccountService() => new(_client);
    private InvoiceService CreateInvoiceService() => new(_client);
    private PaymentIntentService CreatePaymentIntentService() => new(_client);
    private PriceService CreatePriceService() => new(_client);
    private ProductService CreateProductService() => new(_client);
    private SessionService CreateSessionService() => new(_client);
    private SubscriptionService CreateSubscriptionService() => new(_client);
}
