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

public class StripePaymentProvider : IPaymentProvider, IStripeTransactionProvider, IStripeWebhookProvider
{
    /* Wide enough to absorb the gap between an invoice being paid and the webhook that recorded it, and far
       narrower than the shortest billing period, so it cannot span two invoices of one subscription. */
    private static readonly TimeSpan SubscriptionInvoiceMatchWindow = TimeSpan.FromHours(6);

    /* A transfer against a charge is made seconds after it, so this is slack rather than a real expectation
       - wide enough that a transfer delayed by retries is still found, and narrow enough that the search
       stays one page of a single group's transfers. */
    private static readonly TimeSpan TransferSearchWindow = TimeSpan.FromDays(7);

    private readonly ILoggingService _loggingService;
    private readonly IPlatformProvider _platformProvider;
    private readonly StripePaymentProviderSettings _settings;
    private readonly Lazy<AccountLinkService> _stripeAccountLinkService;
    private readonly Lazy<AccountService> _stripeAccountService;
    private readonly Lazy<ChargeService> _stripeChargeService;
    private readonly Lazy<InvoiceService> _stripeInvoiceService;
    private readonly Lazy<PaymentIntentService> _stripePaymentIntentService;
    private readonly Lazy<PriceService> _stripePriceService;
    private readonly Lazy<ProductService> _stripeProductService;
    private readonly Lazy<RefundService> _stripeRefundService;
    private readonly Lazy<SessionService> _stripeSessionService;
    private readonly Lazy<SubscriptionService> _stripeSubscriptionService;
    private readonly Lazy<TransferReversalService> _stripeTransferReversalService;
    private readonly Lazy<TransferService> _stripeTransferService;
    private readonly Lazy<WebhookEndpointService> _stripeWebhookEndpointService;

    public StripePaymentProvider(
        ILoggingService loggingService,
        StripePaymentProviderSettings settings,
        IPlatformProvider platformProvider,
        PlatformType platform)
    {
        var client = new StripeClient(new StripeClientOptions
        {
            ApiKey = settings.Platforms[platform].SecretApiKey
        });
        _loggingService = loggingService;
        _platformProvider = platformProvider;
        _settings = settings;

        _stripeAccountLinkService = new(() => new AccountLinkService(client));
        _stripeAccountService = new(() => new AccountService(client));
        _stripeChargeService = new(() => new ChargeService(client));
        _stripeInvoiceService = new(() => new InvoiceService(client));
        _stripePaymentIntentService = new(() => new PaymentIntentService(client));
        _stripePriceService = new(() => new PriceService(client));
        _stripeProductService = new(() => new ProductService(client));
        _stripeRefundService = new(() => new RefundService(client));
        _stripeSessionService = new(() => new SessionService(client));
        _stripeSubscriptionService = new(() => new SubscriptionService(client));
        _stripeTransferReversalService = new(() => new TransferReversalService(client));
        _stripeTransferService = new(() => new TransferService(client));
        _stripeWebhookEndpointService = new(() => new WebhookEndpointService(client));
    }

    public decimal CommissionPercentage => _settings.ConnectedAccountCommissionPercentage;

    public TimeSpan SettlementReadDelay => _settings.SettlementReadDelay;

    public PaymentProviderType Type => PaymentProviderType.Stripe;

    public async Task<ServiceResult> ActivateSubscriptionPlan(string externalId)
    {
        var service = _stripePriceService.Value;

        await service.UpdateAsync(externalId, new PriceUpdateOptions
        {
            Active = true
        });

        return ServiceResult.Successful();
    }

    public async Task<bool> CancelSubscription(string externalId)
    {
        await _loggingService.Info($"Cancelling Stripe subscription '{externalId}'");

        var service = _stripeSubscriptionService.Value;

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

        var service = _stripeAccountService.Value;

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

    public async Task<string?> CreateSubscriptionPlan(ExternalSubscriptionPlan subscriptionPlan)
    {
        var service = _stripePriceService.Value;

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

    public async Task<CreateTransferResult> CreateTransfer(ExternalTransfer transfer)
    {
        var service = _stripeTransferService.Value;

        try
        {
            /* SourceTransaction ties the transfer to the charge it comes out of, which lets Stripe move
               funds that have not finished clearing and keeps the pair reconcilable at their end.

               The idempotency key is what makes a retry safe: Stripe returns the transfer it already made
               rather than making a second one, so a job that fails after the money moved cannot pay twice. */
            var created = await service.CreateAsync(
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

            return CreateTransferResult.Transferred(created.Id);
        }
        catch (Exception ex)
        {
            var message =
                $"Error transferring {transfer.Amount} {transfer.CurrencyCode} from Stripe charge " +
                $"'{transfer.ExternalChargeId}' to connected account '{transfer.ConnectedAccountId}'";

            await _loggingService.Error(message, ex);
            return CreateTransferResult.Failure(message);
        }
    }

    public async Task<ServiceResult> DeactivateSubscriptionPlan(string externalId)
    {
        var service = _stripePriceService.Value;

        await service.UpdateAsync(externalId, new PriceUpdateOptions
        {
            Active = false
        });

        return ServiceResult.Successful();
    }

    public async Task<string?> FindTransferIdForCharge(
        string externalChargeId, string connectedAccountId, DateTime chargedUtc)
    {
        var service = _stripeTransferService.Value;

        try
        {
            var options = new TransferListOptions
            {
                Destination = connectedAccountId,
                Created = new DateRangeOptions
                {
                    GreaterThanOrEqual = chargedUtc.Subtract(TransferSearchWindow),
                    LessThanOrEqual = chargedUtc.Add(TransferSearchWindow)
                }
            };

            await foreach (var transfer in service.ListAutoPagingAsync(options))
            {
                if (string.Equals(transfer.SourceTransactionId, externalChargeId, StringComparison.Ordinal))
                {
                    return transfer.Id;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            // Warned rather than thrown, because the caller reads null as "none found" and carries on.
            await _loggingService.Warn(
                $"Could not search Stripe transfers to '{connectedAccountId}' for charge " +
                $"'{externalChargeId}': {ex.Message}");
            return null;
        }
    }

    public async Task<string?> GenerateConnectedAccountSetupUrl(GenerateRemoteAccountSetupUrlOptions options)
    {
        await _loggingService.Info($"Refreshing connected stripe account for Stripe account '{options.Id}'");

        var service = _stripeAccountLinkService.Value;

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

    public async Task<ExternalCharge?> GetCharge(string externalChargeId)
    {
        Charge charge;

        try
        {
            charge = await _stripeChargeService.Value.GetAsync(externalChargeId);
        }
        catch (Exception ex)
        {
            await _loggingService.Error($"Error fetching Stripe charge '{externalChargeId}'", ex);
            return null;
        }

        var refunds = new List<ExternalRefund>();

        // Only asked for where there is something to find: an unrefunded charge lists nothing.
        if (charge.AmountRefunded > 0)
        {
            var options = new RefundListOptions
            {
                Charge = charge.Id
            };

            await foreach (var refund in _stripeRefundService.Value.ListAutoPagingAsync(options))
            {
                refunds.Add(ToExternalRefund(refund));
            }
        }

        return new ExternalCharge
        {
            Amount = FromStripeAmount(charge.Amount),
            Commission = FromStripeAmount(charge.ApplicationFeeAmount),
            ExternalId = charge.Id,
            Refunds = refunds
        };
    }

    public async Task<ExternalCheckoutSession?> GetCheckoutSession(string externalId)
    {
        var service = _stripeSessionService.Value;

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
        var service = _stripeAccountService.Value;

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

    public async Task<string> GetOrCreateChapterProduct(Chapter chapter)
    {
        var productName = chapter.FullName;
        return await GetOrCreateProduct(productName);
    }

    public async Task<string> GetOrCreatePlatformProduct(PlatformType platform)
    {
        var productName = $"{_platformProvider.GetName(platform)} Platform";
        return await GetOrCreateProduct(productName);
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

        var service = _stripeInvoiceService.Value;

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
        var service = _stripeInvoiceService.Value;

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
        var service = _stripePaymentIntentService.Value;

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

    public string GetPublicApiKey(PlatformType platform) => _settings.Platforms[platform].PublicApiKey;

    public async Task<ExternalSubscription?> GetSubscription(string externalId)
    {
        if (!externalId.StartsWith("sub_"))
        {
            return null;
        }

        var service = _stripeSubscriptionService.Value;

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
        var service = _stripePriceService.Value;

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

    public async Task<IReadOnlyCollection<StripeSubscription>> ListSubscriptions()
    {
        var service = _stripeSubscriptionService.Value;

        var subscriptions = new List<StripeSubscription>();

        await foreach (var subscription in service.ListAutoPagingAsync(new SubscriptionListOptions
        {
            // Without this Stripe answers with the running ones only, and a cancelled subscription is
            // exactly the sort whose metadata explains a renewal that was never recorded.
            Status = StripeSubscriptionStatuses.All
        }))
        {
            subscriptions.Add(new StripeSubscription
            {
                CreatedUtc = subscription.Created,
                CustomerId = subscription.CustomerId,
                Id = subscription.Id,
                Metadata = subscription.Metadata ?? new Dictionary<string, string>(),
                Status = ToSubscriptionStatus(subscription.Status)
            });
        }

        return subscriptions;
    }

    public async Task<IReadOnlyCollection<StripeTransaction>> ListTransactions()
    {
        var transactions = new List<StripeTransaction>();
        var invoicedPaymentIntentIds = new HashSet<string>(StringComparer.Ordinal);

        /* Invoices first, and expanded: an invoice is the only object naming both the subscription that
           billed it and the payment that settled it - neither a charge nor a payment intent names an invoice
           in this API version - and its payments are not returned unless asked for. */
        await foreach (var invoice in _stripeInvoiceService.Value.ListAutoPagingAsync(new InvoiceListOptions
        {
            Expand = ["data.payments"]
        }))
        {
            var subscriptionDetails = invoice.Parent?.SubscriptionDetails;

            // An invoice retried after a failure names every attempt, so the paid one is preferred and the
            // most recent of those breaks the tie.
            var payment = invoice.Payments?.Data
                .OrderByDescending(x => x.Status == StripeInvoicePaymentStatuses.Paid)
                .ThenByDescending(x => x.Created)
                .FirstOrDefault()
                ?.Payment;

            if (!string.IsNullOrEmpty(payment?.PaymentIntentId))
            {
                invoicedPaymentIntentIds.Add(payment.PaymentIntentId);
            }

            transactions.Add(new StripeTransaction
            {
                Amount = FromStripeAmount(invoice.AmountPaid),
                ChargeId = payment?.ChargeId,
                CreatedUtc = invoice.Created,
                CurrencyCode = invoice.Currency,
                InvoiceId = invoice.Id,
                Kind = ToTransactionKind(invoice),
                /* The subscription details' metadata and nothing else where a subscription billed the
                   invoice, because that is the one an invoice.payment_succeeded webhook reads - see
                   StripeWebhookParser. An invoice raised by anything else falls back to its own. */
                Metadata = subscriptionDetails?.Metadata
                    ?? invoice.Metadata
                    ?? new Dictionary<string, string>(),
                PaidUtc = invoice.StatusTransitions?.PaidAt,
                PaymentIntentId = payment?.PaymentIntentId,
                Status = ToTransactionStatus(invoice),
                SubscriptionId = subscriptionDetails?.SubscriptionId
            });
        }

        /* Then the payment intents no invoice claimed, which is what a one-off is: nothing on a payment
           intent says whether an invoice raised it, so the ones that were are subtracted. */
        await foreach (var paymentIntent in _stripePaymentIntentService.Value.ListAutoPagingAsync(
            new PaymentIntentListOptions()))
        {
            if (invoicedPaymentIntentIds.Contains(paymentIntent.Id))
            {
                continue;
            }

            transactions.Add(new StripeTransaction
            {
                Amount = FromStripeAmount(paymentIntent.Amount),
                ChargeId = paymentIntent.LatestChargeId,
                CreatedUtc = paymentIntent.Created,
                CurrencyCode = paymentIntent.Currency,
                InvoiceId = null,
                Kind = StripeTransactionKind.OneOff,
                Metadata = paymentIntent.Metadata ?? new Dictionary<string, string>(),
                // Nothing on a payment intent says when it was taken - only the charge behind it does, and
                // that is not returned unless expanded.
                PaidUtc = null,
                PaymentIntentId = paymentIntent.Id,
                Status = ToTransactionStatus(paymentIntent),
                SubscriptionId = null
            });
        }

        return transactions;
    }

    public async Task<IReadOnlyCollection<StripeWebhookEndpoint>> ListWebhooks()
    {
        var service = _stripeWebhookEndpointService.Value;

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

    public async Task<ExternalRefund?> RefundCharge(
        string externalChargeId, decimal amount)
    {
        try
        {
            var refund = await _stripeRefundService.Value.CreateAsync(new RefundCreateOptions
            {
                Amount = ToStripeAmount(amount),
                Charge = externalChargeId
            });

            return ToExternalRefund(refund);
        }
        catch (Exception ex)
        {
            await _loggingService.Error($"Error refunding Stripe charge '{externalChargeId}'", ex);
            return null;
        }
    }

    public async Task<ExternalTransferReversal?> ReverseTransfer(string externalTransferId, decimal amount)
    {
        try
        {
            var reversal = await _stripeTransferReversalService.Value.CreateAsync(
                externalTransferId,
                new TransferReversalCreateOptions
                {
                    Amount = ToStripeAmount(amount)
                });

            return new ExternalTransferReversal
            {
                Amount = FromStripeAmount(reversal.Amount),
                CreatedUtc = reversal.Created,
                CurrencyCode = reversal.Currency,
                ExternalId = reversal.Id
            };
        }
        catch (Exception ex)
        {
            await _loggingService.Error($"Error reversing Stripe transfer '{externalTransferId}'", ex);
            return null;
        }
    }

    public async Task<ExternalCheckoutSession> StartCheckout(
        IServiceRequest request,
        string emailAddress,
        ExternalSubscriptionPlan subscriptionPlan,
        string returnPath,
        PaymentMetadataModel metadata,
        ChapterPaymentAccount? chapterPaymentAccount)
    {
        var returnUrl = UrlUtils.Url(
            baseUrl: request.HttpRequestContext.BaseUrl,
            path: returnPath.Replace("{sessionId}", "{CHECKOUT_SESSION_ID}"));

        var metadataDictionary = new Dictionary<string, string>(metadata.ToDictionary());

        var service = _stripeSessionService.Value;

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
                OnBehalfOf = chapterPaymentAccount?.ExternalId
            } : null,
            SubscriptionData = subscriptionPlan.Recurring ? new SessionSubscriptionDataOptions
            {
                Metadata = metadataDictionary,
                OnBehalfOf = chapterPaymentAccount?.ExternalId
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
            ChargedUtc = charge.Created,
            ChargeId = charge.Id,
            CollectedCommissionAmount = charge.ApplicationFeeAmount != null
                ? FromStripeAmount(charge.ApplicationFeeAmount)
                : null,
            CurrencyCode = charge.Currency,
            FeeAmount = balanceTransaction != null ? FromStripeAmount(balanceTransaction.Fee) : null,
            NetAmount = balanceTransaction != null ? FromStripeAmount(balanceTransaction.Net) : null,
            SettlementCurrencyCode = balanceTransaction?.Currency,
            TransferId = charge.Transfer?.Id,
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

    private static ExternalRefund ToExternalRefund(Refund refund) => new()
    {
        Amount = FromStripeAmount(refund.Amount),
        CreatedUtc = refund.Created,
        CurrencyCode = refund.Currency,
        ExternalId = refund.Id,
        Status = ToRefundStatus(refund.Status)
    };

    /* Stripe's requires_action reads as pending: the refund exists and has not moved, which is what a
       pending refund means here, and the action it wants is taken in Stripe's own dashboard. Anything
       unrecognised is pending too, so a status we have not seen leaves the refund open to be read again
       rather than declared finished. */
    private static PaymentRefundStatusType ToRefundStatus(string status) => status switch
    {
        "succeeded" => PaymentRefundStatusType.Refunded,
        "failed" => PaymentRefundStatusType.Failed,
        "canceled" => PaymentRefundStatusType.Cancelled,
        _ => PaymentRefundStatusType.Pending
    };

    private static long ToStripeAmount(decimal amount) => (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);

    private static StripeSubscriptionStatus ToSubscriptionStatus(string status) => status switch
    {
        StripeSubscriptionStatuses.Active => StripeSubscriptionStatus.Active,
        StripeSubscriptionStatuses.Canceled => StripeSubscriptionStatus.Cancelled,
        StripeSubscriptionStatuses.Incomplete => StripeSubscriptionStatus.Incomplete,
        StripeSubscriptionStatuses.IncompleteExpired => StripeSubscriptionStatus.IncompleteExpired,
        StripeSubscriptionStatuses.PastDue => StripeSubscriptionStatus.PastDue,
        StripeSubscriptionStatuses.Paused => StripeSubscriptionStatus.Paused,
        StripeSubscriptionStatuses.Trialing => StripeSubscriptionStatus.Trialing,
        StripeSubscriptionStatuses.Unpaid => StripeSubscriptionStatus.Unpaid,
        _ => StripeSubscriptionStatus.None
    };

    /* An unrecognised billing reason is read as a renewal, which is the safe way round: a renewal's metadata
       is audited, and a first invoice's is the one already known to have come from checkout. */
    private static StripeTransactionKind ToTransactionKind(Invoice invoice)
        => invoice.Parent?.SubscriptionDetails?.SubscriptionId == null
            ? StripeTransactionKind.OneOff
            : invoice.BillingReason == StripeInvoiceBillingReasons.SubscriptionCreate
                ? StripeTransactionKind.SubscriptionInitial
                : StripeTransactionKind.SubscriptionRenewal;

    private static StripeTransactionStatus ToTransactionStatus(Invoice invoice) => invoice.Status switch
    {
        StripeInvoiceStatuses.Paid => StripeTransactionStatus.Succeeded,
        StripeInvoiceStatuses.Uncollectible or StripeInvoiceStatuses.Void => StripeTransactionStatus.Cancelled,
        _ => StripeTransactionStatus.Pending
    };

    private static StripeTransactionStatus ToTransactionStatus(PaymentIntent paymentIntent)
        => paymentIntent.Status switch
        {
            StripePaymentIntentStatuses.Succeeded => StripeTransactionStatus.Succeeded,
            StripePaymentIntentStatuses.Canceled => StripeTransactionStatus.Cancelled,
            _ => StripeTransactionStatus.Pending
        };

    private string? CleanConnectedAccountUrl(PlatformType platform, string url)
    {
        // do not send localhost to Stripe
        var baseUrl = UrlUtils.BaseUrl(url);
        if (_settings.Platforms.TryGetValue(platform, out var platformSettings) &&
            !string.IsNullOrEmpty(platformSettings.ConnectedAccountBaseUrl))
        {
            return url.Replace(baseUrl, platformSettings.ConnectedAccountBaseUrl, StringComparison.OrdinalIgnoreCase);
        }

        return baseUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase)
            ? baseUrl
            : null;
    }

    private async Task<string> CreateProduct(string name)
    {
        var service = _stripeProductService.Value;
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

    private async Task<string> GetOrCreateProduct(string name)
    {
        var service = _stripeProductService.Value;
        var products = await service.ListAsync();
        var existing = products
            .FirstOrDefault(x => string.Equals(name, x.Name, StringComparison.OrdinalIgnoreCase))
            ?.Id;
        if (!string.IsNullOrEmpty(existing))
        {
            return existing;
        }

        return await CreateProduct(name);
    }
}