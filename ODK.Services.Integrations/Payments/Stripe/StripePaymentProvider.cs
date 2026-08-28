using ODK.Core.Chapters;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Core.Utils;
using ODK.Services.Logging;
using ODK.Services.Payments;
using ODK.Services.Payments.Models;
using ODK.Services.Platforms;
using Stripe;
using Stripe.Checkout;

namespace ODK.Services.Integrations.Payments.Stripe;

public class StripePaymentProvider : IPaymentProvider, IStripeWebhookProvider
{
    /* Wide enough to absorb the gap between an invoice being paid and the webhook that recorded it, and far
       narrower than the shortest billing period, so it cannot span two invoices of one subscription. */
    private static readonly TimeSpan SubscriptionInvoiceMatchWindow = TimeSpan.FromHours(6);

    private readonly IStripeClient _client;
    private readonly string? _connectedAccountId;
    private readonly ILoggingService _loggingService;
    private readonly IPlatformProvider _platformProvider;
    private readonly StripePaymentProviderSettings _settings;

    public StripePaymentProvider(
        SitePaymentSettings paymentSettings,
        ILoggingService loggingService,
        string? connectedAccountId,
        StripePaymentProviderSettings settings,
        IPlatformProvider platformProvider)
    {
        _client = new StripeClient(new StripeClientOptions
        {
            ApiKey = paymentSettings.ApiSecretKey
        });
        _connectedAccountId = connectedAccountId;
        _loggingService = loggingService;
        _platformProvider = platformProvider;
        _settings = settings;
    }

    public decimal CommissionPercentage => _settings.ConnectedAccountCommissionPercentage;

    public TimeSpan SettlementReadDelay => _settings.SettlementReadDelay;

    public PaymentProviderType Type => PaymentProviderType.Stripe;

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

    public async Task<RemoteAccount?> CreateConnectedAccount(RemoteAccountCreateOptions options)
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
                    Name = GetConnectedAccountBusinessName(options.Chapter),
                    Url = CleanConnectedAccountUrl(options.Chapter.Platform, options.ChapterUrl),
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

    public async Task<ServiceResult> CreateTransfer(ExternalTransfer transfer)
    {
        var service = CreateTransferService();

        try
        {
            /* SourceTransaction ties the transfer to the charge it comes out of, which lets Stripe move
               funds that have not finished clearing and keeps the pair reconcilable at their end.

               The idempotency key is what makes a retry safe: Stripe returns the transfer it already made
               rather than making a second one, so a job that fails after the money moved cannot pay twice. */
            await service.CreateAsync(
                new TransferCreateOptions
                {
                    Amount = ToStripeAmount(transfer.Amount),
                    Currency = transfer.CurrencyCode.ToLowerInvariant(),
                    Destination = transfer.ConnectedAccountId,
                    SourceTransaction = transfer.ExternalChargeId
                },
                new RequestOptions
                {
                    IdempotencyKey = transfer.IdempotencyKey
                });

            return ServiceResult.Successful();
        }
        catch (Exception ex)
        {
            var message =
                $"Error transferring {transfer.Amount} {transfer.CurrencyCode} from Stripe charge " +
                $"'{transfer.ExternalChargeId}' to connected account '{transfer.ConnectedAccountId}'";

            await _loggingService.Error(message, ex);
            return ServiceResult.Failure(message);
        }
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
            var session = await service.GetAsync(externalId, new SessionGetOptions
            {
                Expand =
                [
                    "invoice",
                    "payment_intent.latest_charge"
                ]
            });

            DateTime? completedUtc = null;
            if (session.PaymentStatus == "paid")
            {
                completedUtc = session.PaymentIntent?.LatestCharge?.Created
                    ?? session.Invoice?.Created;
            }

            return new ExternalCheckoutSession
            {
                Amount = session.AmountTotal ?? 0,
                ClientSecret = session.ClientSecret,
                CompletedUtc = completedUtc,
                Currency = session.Currency,
                Metadata = session.Metadata ?? [],
                PaymentId = session.PaymentIntentId,
                SessionId = session.Id,
                SubscriptionId = session.SubscriptionId
            };
        }
        catch (Exception ex)
        {
            await _loggingService.Error($"Error retrieving Stripe checkout session '{externalId}'", ex);
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

    public async Task<string?> GetPaymentIdForReference(string reference, DateTime paidUtc)
    {
        if (reference.StartsWith(StripeIdPrefixes.PaymentIntent, StringComparison.Ordinal))
        {
            return reference;
        }

        if (!reference.StartsWith(StripeIdPrefixes.Subscription, StringComparison.Ordinal))
        {
            await _loggingService.Warn(
                $"Stripe reference '{reference}' names neither a payment nor a subscription");
            return null;
        }

        var service = CreateInvoiceService();

        try
        {
            /* A subscription bills months apart, so the invoice paid when the payment was recorded is the
               one that produced it. Matched on time rather than amount, which repeats every renewal, and
               required to be the only one in the window: two candidates mean the wrong one could be picked,
               and a wrong figure in an accounting column is worse than an absent one. */
            var invoices = await service.ListAsync(new InvoiceListOptions
            {
                Subscription = reference,
                Limit = 100
            });

            var matches = invoices.Data
                .Where(x => x.StatusTransitions?.PaidAt != null &&
                    (x.StatusTransitions.PaidAt.Value - paidUtc).Duration() < SubscriptionInvoiceMatchWindow)
                .ToArray();

            if (matches.Length != 1)
            {
                await _loggingService.Warn(
                    $"Stripe subscription '{reference}' has {matches.Length} invoices paid within " +
                    $"{SubscriptionInvoiceMatchWindow} of {paidUtc:o}; cannot identify one payment");
                return null;
            }

            return await GetInvoicePaymentId(matches[0].Id);
        }
        catch (Exception ex)
        {
            /* Warned rather than logged as an error, because the caller reads null as "not here" and
               carries on. A failure that genuinely matters is thrown by the caller and recorded once, on
               the final retry, by HangfireJobFailureLoggerAttribute - see PaymentService. The message
               carries the provider's own reason, which is the part worth keeping. */
            await _loggingService.Warn(
                $"Could not list invoices for Stripe subscription '{reference}': {ex.Message}");
            return null;
        }
    }

    public async Task<string?> GetInvoicePaymentId(string externalInvoiceId)
    {
        var service = CreateInvoiceService();

        try
        {
            /* The payments an invoice was settled by are not on the invoice unless asked for, and the
               payment intent sits two levels inside them, so this cannot be read off the webhook the
               invoice arrived on. */
            var invoice = await service.GetAsync(externalInvoiceId, new InvoiceGetOptions
            {
                Expand = ["payments"]
            });

            var paymentId = invoice.Payments?.Data
                .Where(x => x.Status == StripeInvoicePaymentStatuses.Paid)
                .Select(x => x.Payment?.PaymentIntentId)
                .FirstOrDefault(x => !string.IsNullOrEmpty(x));

            if (string.IsNullOrEmpty(paymentId))
            {
                await _loggingService.Warn(
                    $"Stripe invoice '{externalInvoiceId}' names no paid payment intent");
            }

            return paymentId;
        }
        catch (Exception ex)
        {
            /* Warned rather than logged as an error, because the caller reads null as "not here" and
               carries on. A failure that genuinely matters is thrown by the caller and recorded once, on
               the final retry, by HangfireJobFailureLoggerAttribute - see PaymentService. The message
               carries the provider's own reason, which is the part worth keeping. */
            await _loggingService.Warn(
                $"Could not retrieve Stripe invoice '{externalInvoiceId}': {ex.Message}");
            return null;
        }
    }

    public async Task<ExternalPaymentSettlement?> GetPaymentSettlement(string externalPaymentId)
    {
        var service = CreatePaymentIntentService();

        try
        {
            var paymentIntent = await service.GetAsync(externalPaymentId, new PaymentIntentGetOptions
            {
                Expand =
                [
                    "latest_charge.balance_transaction",
                    "latest_charge.transfer"
                ]
            });

            if (paymentIntent.LatestCharge == null)
            {
                await _loggingService.Warn(
                    $"Stripe payment intent '{externalPaymentId}' has no charge; cannot read what it settled");
                return null;
            }

            return MapSettlement(paymentIntent.LatestCharge);
        }
        catch (Exception ex)
        {
            /* Warned rather than logged as an error, because the caller reads null as "not here" and
               carries on. A failure that genuinely matters is thrown by the caller and recorded once, on
               the final retry, by HangfireJobFailureLoggerAttribute - see PaymentService. The message
               carries the provider's own reason, which is the part worth keeping. */
            await _loggingService.Warn(
                $"Could not retrieve Stripe payment intent '{externalPaymentId}': {ex.Message}");
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

    public async Task<ExternalSubscription?> GetSubscription(string externalId)
    {
        if (!externalId.StartsWith("sub_"))
        {
            return null;
        }

        var service = CreateSubscriptionService();

        try
        {
            var subscription = await service.GetAsync(externalId);
            return await MapSubscription(subscription);
        }
        catch
        {
            await _loggingService.Warn($"Error retrieving Stripe subscription '{externalId}'");
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
            await _loggingService.Warn($"Error retrieving Stripe subscription plan '{externalId}'");
            return null;
        }
    }

    public async Task<IReadOnlyCollection<StripeWebhookEndpoint>> ListWebhooks()
    {
        var service = CreateWebhookEndpointService();

        var endpoints = new List<StripeWebhookEndpoint>();

        /* Auto-paged rather than a single call with a limit: an account is expected to have two endpoints, so
           a page boundary is not a real prospect, but a cap that silently drops the rest would read as an
           account having fewer endpoints than it does. */
        await foreach (var endpoint in service.ListAutoPagingAsync())
        {
            endpoints.Add(new StripeWebhookEndpoint
            {
                ApiVersion = endpoint.ApiVersion,
                Description = endpoint.Description,
                Enabled = endpoint.Status == StripeWebhookEndpointStatuses.Enabled,
                Events = endpoint.EnabledEvents?.ToArray() ?? [],
                Id = endpoint.Id,
                LiveMode = endpoint.Livemode,
                Url = endpoint.Url
            });
        }

        return endpoints;
    }

    public async Task<ExternalCheckoutSession> StartCheckout(
        IServiceRequest request,
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

        /* A group's payments settle against its own connected account: OnBehalfOf makes that account the
           settlement merchant, so the customer's statement carries the group's business name rather than the
           platform's, and the charge settles in the group's country and currency.

           Deliberately no TransferData and no application fee. Those would move the money as the charge is
           made, before Stripe has said what the charge cost - and our commission comes out of the net, which
           is not knowable until then, because the fee depends on the card the member chooses. So the whole
           charge is collected here and PaymentService transfers the group's share once the fee is known. */

        var session = await service.CreateAsync(new SessionCreateOptions
        {
            // "elements" renders the payment fields as Stripe Elements inside our own page (see the
            // _StripeCheckout component), rather than Stripe's own embedded checkout page. The session is
            // otherwise identical - same line items, mode, return_url and Connect settings.
            UiMode = "elements",
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
                Metadata = metadataDictionary,
                OnBehalfOf = _connectedAccountId
            } : null,
            SubscriptionData = subscriptionPlan.Recurring ? new SessionSubscriptionDataOptions
            {
                Metadata = metadataDictionary,
                OnBehalfOf = _connectedAccountId
            } : null
        });

        return new ExternalCheckoutSession
        {
            Amount = session.AmountTotal ?? 0,
            ClientSecret = session.ClientSecret,
            CompletedUtc = null,
            Currency = session.Currency,
            Metadata = metadataDictionary,
            PaymentId = null,
            SessionId = session.Id,
            SubscriptionId = null
        };
    }

    /* The fee and the net live on the charge's balance transaction, in the currency the money settled into
       - which is not necessarily the currency charged, so the code travels with them. An absent balance
       transaction means Stripe has not settled the charge yet, which is a state to wait out rather than a
       charge that cost nothing, so the two amounts stay null instead of reading as zero. A balance
       transaction of status "pending" is settled for this purpose: pending is about when the funds become
       available to pay out, and the fee is known from the start.

       A charge is collected whole and the group's share transferred afterwards, so ordinarily nothing here
       says what either party keeps. A charge made before that changed carries an application fee and a
       transfer of its own, and those are reported rather than recomputed: what happened to it is a matter
       of record, not something a current commission rate can be applied to. */
    internal static ExternalPaymentSettlement MapSettlement(Charge charge)
    {
        var balanceTransaction = charge.BalanceTransaction;

        return new ExternalPaymentSettlement
        {
            Amount = FromStripeAmount(charge.Amount),
            ChargeId = charge.Id,
            CollectedCommissionAmount = charge.ApplicationFeeAmount != null
                ? FromStripeAmount(charge.ApplicationFeeAmount)
                : null,
            CurrencyCode = charge.Currency,
            FeeAmount = balanceTransaction != null ? FromStripeAmount(balanceTransaction.Fee) : null,
            NetAmount = balanceTransaction != null ? FromStripeAmount(balanceTransaction.Net) : null,
            SettlementCurrencyCode = balanceTransaction?.Currency,
            TransferredUtc = charge.Transfer?.Created
        };
    }

    // The name is built from the platform the group belongs to rather than the platform serving the
    // request, because the business name belongs to the group. Drunken Knitwits groups carry their own
    // identity in their full name, so the configured template applies only to the other platform.
    internal string GetConnectedAccountBusinessName(Chapter chapter)
        => chapter.Platform == PlatformType.DrunkenKnitwits
            ? chapter.FullName
            : StringUtils.Interpolate(_settings.ConnectedAccountBusinessName, new Dictionary<string, string>
            {
                { "platform.title", _platformProvider.GetName(chapter.Platform) },
                { "group.name", chapter.Name }
            });

    // The period dates and the plan (price) id live on the subscription's item(s) rather than the
    // top-level Subscription in current Stripe API versions. Standard single-plan subscriptions have
    // exactly one item, so we read the first. Without an item the essential fields (plan id, billing
    // dates) are missing, so we return null rather than a useless instance. LastPaymentDate is
    // approximated by the current period start - kept for auditing/debugging even though nothing
    // currently reads it.
    internal async Task<ExternalSubscription?> MapSubscription(Subscription subscription)
    {
        var item = subscription.Items?.Data?.FirstOrDefault();
        if (item == null)
        {
            await _loggingService.Error(
                $"Stripe subscription '{subscription.Id}' returned no items; " +
                $"cannot resolve plan or billing dates");

            return null;
        }

        return new ExternalSubscription
        {
            CancelDate = subscription.CancelAt,
            ConnectedAccountId = subscription.TransferData?.DestinationId,
            ExternalId = subscription.Id,
            ExternalSubscriptionPlanId = item.Price?.Id ?? string.Empty,
            LastPaymentDate = item.CurrentPeriodStart,
            Metadata = subscription.Metadata,
            NextBillingDate = item.CurrentPeriodEnd,
            Status = subscription.Status == "active" && subscription.CancelAt == null
                ? ExternalSubscriptionStatus.Active
                : ExternalSubscriptionStatus.Cancelled
        };
    }

    private static decimal FromStripeAmount(long? stripeAmount) => (stripeAmount ?? 0) / 100m;

    private static long ToStripeAmount(decimal amount) => (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);

    private string? CleanConnectedAccountUrl(PlatformType platform, string url)
    {
        // do not send localhost to Stripe
        var baseUrl = UrlUtils.BaseUrl(url);
        _settings.ConnectedAccountBaseUrls.TryGetValue(platform, out var connectedAccountBaseUrl);
        if (!string.IsNullOrEmpty(connectedAccountBaseUrl))
        {
            return url.Replace(baseUrl, connectedAccountBaseUrl, StringComparison.OrdinalIgnoreCase);
        }

        return baseUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase)
            ? baseUrl
            : null;
    }

    private AccountLinkService CreateAccountLinkService() => new(_client);

    private AccountService CreateAccountService() => new(_client);

    private InvoiceService CreateInvoiceService() => new(_client);

    private PaymentIntentService CreatePaymentIntentService() => new(_client);

    private PriceService CreatePriceService() => new(_client);

    private ProductService CreateProductService() => new(_client);

    private SessionService CreateSessionService() => new(_client);

    private SubscriptionService CreateSubscriptionService() => new(_client);

    private TransferService CreateTransferService() => new(_client);

    private WebhookEndpointService CreateWebhookEndpointService() => new(_client);
}