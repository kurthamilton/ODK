using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Events;
using ODK.Services.Exceptions;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.Payments.Models;
using ODK.Services.Platforms;
using ODK.Services.Subscriptions;
using ODK.Services.Tasks;

namespace ODK.Services.Payments;

public class PaymentService : IPaymentService
{
    private readonly IBackgroundTaskService _backgroundTaskService;
    private readonly IEventService _eventService;
    private readonly ILoggingService _loggingService;
    private readonly IMemberChapterSubscriptionWriter _memberChapterSubscriptionWriter;
    private readonly IMemberEmailService _memberEmailService;
    private readonly IMemberSiteSubscriptionWriter _memberSiteSubscriptionWriter;
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly IPlatformProvider _platformProvider;
    private readonly IServiceRequestFactory _serviceRequestFactory;
    private readonly SiteSubscriptionCooldown _siteSubscriptionCooldown;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(
        IUnitOfWork unitOfWork,
        ILoggingService loggingService,
        IMemberEmailService memberEmailService,
        IPaymentProviderFactory paymentProviderFactory,
        IEventService eventService,
        IBackgroundTaskService backgroundTaskService,
        IMemberChapterSubscriptionWriter memberChapterSubscriptionWriter,
        IMemberSiteSubscriptionWriter memberSiteSubscriptionWriter,
        IPlatformProvider platformProvider,
        IServiceRequestFactory serviceRequestFactory,
        SiteSubscriptionCooldown siteSubscriptionCooldown)
    {
        _backgroundTaskService = backgroundTaskService;
        _eventService = eventService;
        _loggingService = loggingService;
        _memberChapterSubscriptionWriter = memberChapterSubscriptionWriter;
        _memberEmailService = memberEmailService;
        _memberSiteSubscriptionWriter = memberSiteSubscriptionWriter;
        _paymentProviderFactory = paymentProviderFactory;
        _platformProvider = platformProvider;
        _serviceRequestFactory = serviceRequestFactory;
        _siteSubscriptionCooldown = siteSubscriptionCooldown;
        _unitOfWork = unitOfWork;
    }

    public string EnqueueEnsureProductExistsJob(JobRequest request)
        => _backgroundTaskService.Enqueue(
            () => EnsureProductExistsJob(request),
            BackgroundTaskQueueType.General);

    public string EnqueueProcessWebhookJob(JobRequest request, PaymentProviderWebhook webhook)
        => _backgroundTaskService.Enqueue(
            () => ProcessWebhookJob(request, webhook),
            BackgroundTaskQueueType.Payments);

    public string EnqueueResolvePaymentSettlementJob(Guid paymentId)
        => _backgroundTaskService.Enqueue(
            () => ResolvePaymentSettlementJob(paymentId, null, null),
            BackgroundTaskQueueType.Payments);

    /* Public for Hangfire, which needs a method to bind to, and called by nothing else: it turns the job's
       ids back into a request and hands off to the work. These signatures are a wire format - see JobRequest -
       so a change to one is a change every queued job of that kind has to survive. */
    public async Task EnsureProductExistsJob(JobRequest request)
        => await EnsureProductExists(await _serviceRequestFactory.CreateChapterRequest(request));

    public async Task<PaymentStatusType> GetMemberChapterPaymentCheckoutSessionStatus(
        IMemberServiceRequest request, Guid chapterId, string externalSessionId)
    {
        var (chapterPaymentAccountDto, checkoutSession) = await _unitOfWork.Run(
            x => x.ChapterPaymentAccountRepository.Query().ForChapter(chapterId).ToDto().GetSingle(),
            x => x.PaymentCheckoutSessionRepository.GetByMemberId(request.CurrentMember.Id, externalSessionId));

        OdkAssertions.Exists(checkoutSession);

        if (checkoutSession.CompletedUtc != null)
        {
            return PaymentStatusType.Complete;
        }

        var paymentProvider = _paymentProviderFactory.GetPaymentProvider(
            chapterPaymentAccountDto.SitePaymentSettings,
            chapterPaymentAccountDto.ChapterPaymentAccount);

        // Completion is driven solely by the payment provider webhook; this status check only reports
        // progress. An expired remote session is surfaced so the UI can stop polling.
        var externalSession = await paymentProvider.GetCheckoutSession(externalSessionId);
        if (externalSession == null)
        {
            return PaymentStatusType.Expired;
        }

        return PaymentStatusType.Pending;
    }

    public async Task<PaymentStatusType> GetMemberSitePaymentCheckoutSessionStatus(
        IMemberServiceRequest request, string externalSessionId)
    {
        var (checkoutSession, sitePaymentSettings) = await _unitOfWork.Run(
            x => x.PaymentCheckoutSessionRepository.GetByMemberId(request.CurrentMember.Id, externalSessionId),
            x => x.SitePaymentSettingsRepository.GetAll());

        OdkAssertions.Exists(checkoutSession);

        if (checkoutSession.CompletedUtc != null)
        {
            return PaymentStatusType.Complete;
        }

        var payment = await _unitOfWork.PaymentRepository.GetById(checkoutSession.PaymentId).Run();

        var paymentProvider = _paymentProviderFactory.GetSitePaymentProvider(
            sitePaymentSettings, payment.SitePaymentSettingId);

        // Completion is driven solely by the payment provider webhook; this status check only reports
        // progress. An expired remote session is surfaced so the UI can stop polling.
        var externalSession = await paymentProvider.GetCheckoutSession(externalSessionId);
        if (externalSession == null)
        {
            return PaymentStatusType.Expired;
        }

        return PaymentStatusType.Pending;
    }

    public async Task ProcessWebhook(IServiceRequest request, PaymentProviderWebhook webhook)
    {
        /* The actioning runs as the platform the payment was made on, not the one the webhook arrived at: a
           provider posts every platform's events to whichever endpoint was registered with it, so the host
           that received one says nothing about the payment it describes. That platform decides which site the
           receipt is sent as and which site its links point at, so it is settled here, before the event is
           recorded - a platform with no configured URL throws while the provider will still redeliver. */
        var platform = PaymentMetadataModel.FromDictionary(webhook.Metadata).PlatformOrDrunkenKnitwits;
        var actionRequest = JobRequest.Create(request)
            .ForPlatform(platform, _platformProvider.GetBaseUrl(platform));

        var existingEvent = await _unitOfWork.PaymentProviderWebhookEventRepository
            .GetByExternalId(webhook.PaymentProviderType, webhook.Id).Run();

        if (existingEvent != null)
        {
            await _loggingService.Warn($"{webhook.PaymentProviderType} webhook for event {webhook.Id} already processed");
            return;
        }

        _unitOfWork.PaymentProviderWebhookEventRepository.Add(new PaymentProviderWebhookEvent
        {
            ExternalId = webhook.Id,
            PaymentProviderType = webhook.PaymentProviderType,
            ReceivedUtc = DateTime.UtcNow
        });

        await _unitOfWork.SaveChanges();

        // Run the actioning of the webhook itself in a new task so that we can persist the event as quickly as possible
        // and make the actual processing retryable.
        _backgroundTaskService.Enqueue(
            () => ProcessWebhookActionJob(actionRequest, webhook),
            BackgroundTaskQueueType.Payments);
    }

    // Public for Hangire
    public async Task ProcessWebhookAction(IServiceRequest request, PaymentProviderWebhook webhook)
    {
        PaymentWebhookProcessingResult result;

        switch (webhook.Type)
        {
            case PaymentProviderWebhookType.CheckoutSessionCompleted:
                result = await ProcessWebhookPayment(webhook);
                break;

            case PaymentProviderWebhookType.CheckoutSessionExpired:
                result = await ProcessWebhookCheckoutSessionExpired(webhook);
                break;

            case PaymentProviderWebhookType.InvoicePaymentSucceeded:
            case PaymentProviderWebhookType.SubscriptionCancelled:
                result = await ProcessWebhookSubscription(webhook);
                break;

            default:
                result = PaymentWebhookProcessingResult.Failure();
                break;
        }

        if (!result.Success)
        {
            return;
        }

        if (result.Payment == null)
        {
            return;
        }

        /* Reading back what actually moved is its own job: it is a further call to the provider, and a
           member's access must neither wait on it nor be lost to it failing. Its own job also gets its own
           retries, which is what waits out a payment the provider has not finished settling.

           Scheduled rather than enqueued, so the ordinary payment is read once and needs no retry. The wait
           is the provider's to state - it is the one that knows how long after taking the money it finishes
           moving it - and it is not what makes the settlement correct, only what keeps it quiet. */
        var paymentId = result.Payment.Id;
        var sitePaymentSettings = await _unitOfWork.SitePaymentSettingsRepository.GetAll().Run();
        var settlementReadDelay = _paymentProviderFactory
            .GetSitePaymentProvider(sitePaymentSettings, result.Payment.SitePaymentSettingId)
            .SettlementReadDelay;

        _backgroundTaskService.Schedule(
            () => ResolvePaymentSettlementJob(paymentId, webhook.PaymentId, webhook.InvoiceId),
            DateTime.UtcNow.Add(settlementReadDelay),
            BackgroundTaskQueueType.Payments);

        if (result.Currency != null &&
            result.Member != null)
        {
            var (member, chapter, currency, payment) = (result.Member, result.Chapter, result.Currency, result.Payment);

            await _memberEmailService.SendPaymentNotification(request, member, chapter, payment, currency);
        }
    }

    /// <inheritdoc cref="EnsureProductExistsJob" />
    public async Task ProcessWebhookActionJob(JobRequest request, PaymentProviderWebhook webhook)
        => await ProcessWebhookAction(await _serviceRequestFactory.Create(request), webhook);

    /// <inheritdoc cref="EnsureProductExistsJob" />
    public async Task ProcessWebhookJob(JobRequest request, PaymentProviderWebhook webhook)
        => await ProcessWebhook(await _serviceRequestFactory.Create(request), webhook);

    /// <inheritdoc cref="EnsureProductExistsJob" />
    public async Task ResolvePaymentSettlementJob(
        Guid paymentId, string? externalPaymentId, string? externalInvoiceId)
        => await ResolvePaymentSettlement(paymentId, externalPaymentId, externalInvoiceId);

    // A period that is still live - or lapsed but inside the cooldown - is continued, so a subscription
    // keeps its anniversary instead of drifting by however late the member renewed. Otherwise the period
    // starts now. A cooldown of zero therefore continues only a live period.
    //
    // A cooldown longer than the subscription's own length can continue a period that has already fully
    // elapsed, so a calculated expiry that is not in the future starts a new period instead: a payment must
    // always leave the member current.
    private static string ToTransferIdempotencyKey(Guid paymentId)
        => $"payment-transfer-{paymentId}";

    private static DateTime RollExpiryForward(
        DateTime? currentExpiresUtc,
        int months,
        DateTime cooldownStartUtc,
        DateTime utcNow)
    {
        var continueFromUtc = currentExpiresUtc >= cooldownStartUtc
            ? currentExpiresUtc.Value
            : utcNow;

        var expiresUtc = continueFromUtc.AddMonths(months);

        return expiresUtc > utcNow
            ? expiresUtc
            : utcNow.AddMonths(months);
    }

    private async Task EnsureProductExists(IChapterServiceRequest request)
    {
        var chapter = request.Chapter;

        var (chapterPaymentSettings, sitePaymentSettings) = await _unitOfWork.Run(
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPaymentAccountRepository.Query().ForChapter(chapter.Id).ToSitePaymentSettings().GetSingle());

        if (!string.IsNullOrEmpty(chapterPaymentSettings?.ExternalProductId))
        {
            return;
        }

        var paymentProvider = _paymentProviderFactory.GetSitePaymentProvider(sitePaymentSettings);

        var productName = chapter.FullName;

        var productId = await paymentProvider.GetProductId(productName);
        if (string.IsNullOrEmpty(productId))
        {
            productId = await paymentProvider.CreateProduct(productName);
        }

        if (string.IsNullOrEmpty(productId))
        {
            await _loggingService.Error($"Could not create payment product for chapter {chapter.FullName}");
            return;
        }

        chapterPaymentSettings ??= new ChapterPaymentSettings();

        chapterPaymentSettings.ExternalProductId = productId;

        if (chapterPaymentSettings.ChapterId == default)
        {
            chapterPaymentSettings.ChapterId = chapter.Id;
            _unitOfWork.ChapterPaymentSettingsRepository.Add(chapterPaymentSettings);
        }
        else
        {
            _unitOfWork.ChapterPaymentSettingsRepository.Update(chapterPaymentSettings);
        }

        await _unitOfWork.SaveChanges();
    }

    private async Task<DateTime?> GetChapterSubscriptionNextPaymentDate(
        Guid chapterId, string externalId)
    {
        var chapterPaymentAccountDto = await _unitOfWork.Run(
            x => x.ChapterPaymentAccountRepository.Query().ForChapter(chapterId).ToDto().GetSingle());

        var paymentProvider = _paymentProviderFactory.GetPaymentProvider(
            chapterPaymentAccountDto.SitePaymentSettings, chapterPaymentAccountDto.ChapterPaymentAccount);

        return await GetNextPaymentDate(paymentProvider, externalId);
    }

    private async Task<DateTime?> GetNextPaymentDate(IPaymentProvider paymentProvider, string externalId)
    {
        var externalSubscription = await paymentProvider.GetSubscription(externalId);

        if (externalSubscription?.NextBillingDate == null)
        {
            await _loggingService.Warn(
                $"Next payment date not found for {paymentProvider.Type} subscription '{externalId}'; " +
                $"calculating the expiry date instead");
            return null;
        }

        return externalSubscription.NextBillingDate;
    }

    private async Task<PaymentWebhookProcessingResult> ProcessCompletedChapterSubscription(
        PaymentMetadataModel metadata,
        DateTime completedUtc,
        PaymentProviderType paymentProvider,
        string externalId,
        string? initiatorId)
    {
        var platform = metadata.PlatformOrDrunkenKnitwits;

        if (metadata.MemberId == null ||
            metadata.ChapterId == null ||
            metadata.ChapterSubscriptionId == null)
        {
            var missingProperties = new[]
            {
                metadata.MemberId == null ? "MemberId" : null,
                metadata.ChapterId == null ? "ChapterId" : null,
                metadata.ChapterSubscriptionId == null ? "ChapterSubscriptionId" : null
            }.Where(x => x != null);

            var message =
                $"Cannot process {paymentProvider} completed chapter subscription: " +
                $"metadata missing properties {string.Join(", ", missingProperties)}";

            await _loggingService.Error(message);
            return PaymentWebhookProcessingResult.Failure();
        }

        // Load basic metadata objects
        var (member, chapter, chapterSubscription, payment, paymentCheckoutSession) = await _unitOfWork.Run(
            x => x.MemberRepository.GetById(metadata.MemberId.Value),
            x => x.ChapterRepository.GetById(platform, metadata.ChapterId.Value),
            x => x.ChapterSubscriptionRepository.GetById(metadata.ChapterSubscriptionId.Value),
            x => metadata.PaymentId != null
                ? x.PaymentRepository.GetByIdOrDefault(metadata.PaymentId.Value)
                : new DefaultDeferredQuerySingleOrDefault<Payment>(),
            x => metadata.PaymentCheckoutSessionId != null
                ? x.PaymentCheckoutSessionRepository.GetByIdOrDefault(metadata.PaymentCheckoutSessionId.Value)
                : new DefaultDeferredQuerySingleOrDefault<PaymentCheckoutSession>());

        // A renewal has no checkout Payment: create one (already paid) and add it
        if (payment == null)
        {
            payment = _unitOfWork.PaymentRepository.Add(new Payment
            {
                Amount = chapterSubscription.Amount,
                ChapterId = chapter.Id,
                CreatedUtc = completedUtc,
                CurrencyId = chapterSubscription.CurrencyId,
                ExternalId = externalId,
                Id = _unitOfWork.NewId(),
                MemberId = member.Id,
                PaidUtc = completedUtc,
                Reference = chapterSubscription.ToReference(),
                SitePaymentSettingId = chapterSubscription.SitePaymentSettingId
            });
        }
        else if (payment.PaidUtc != null)
        {
            await _loggingService.Warn(
                $"Not updating Payment {payment.Id} in {paymentProvider} webhook processing: already paid");
        }
        else
        {
            payment.ExternalId = externalId;
            payment.PaidUtc = completedUtc;
            _unitOfWork.PaymentRepository.Update(payment);
        }

        // update payment checkout session
        if (paymentCheckoutSession != null)
        {
            if (paymentCheckoutSession.CompletedUtc != null)
            {
                var message =
                    $"Not updating PaymentCheckoutSession {paymentCheckoutSession.Id} " +
                    $"in {paymentProvider} webhook processing: " +
                    $"already completed";
                await _loggingService.Warn(message);
            }
            else
            {
                paymentCheckoutSession.CompletedUtc = completedUtc;
                _unitOfWork.PaymentCheckoutSessionRepository.Update(paymentCheckoutSession);
            }
        }

        return await UpdateMemberChapterSubscription(
            metadata,
            member,
            payment,
            externalId: externalId,
            completedUtc,
            initiatorId);
    }

    private async Task<PaymentWebhookProcessingResult> ProcessCompletedPayment(
        PaymentMetadataModel metadata,
        DateTime completedUtc,
        PaymentProviderType paymentProvider,
        string externalId,
        string? initiatorId)
    {
        if (metadata.MemberId == null ||
            metadata.PaymentId == null ||
            metadata.PaymentCheckoutSessionId == null)
        {
            var missingProperties = new[]
            {
                metadata.MemberId == null ? "MemberId" : null,
                metadata.PaymentId == null ? "PaymentId" : null,
                metadata.PaymentCheckoutSessionId == null ? "PaymentCheckoutSessionId" : null
            }.Where(x => x != null);

            var message =
                $"Cannot process {paymentProvider} payment: " +
                $"metadata missing properties {string.Join(", ", missingProperties)}";

            await _loggingService.Warn(message);
            return PaymentWebhookProcessingResult.Failure();
        }

        // Load basic metadata objects
        var (member, payment, paymentCheckoutSession) = await _unitOfWork.Run(
            x => x.MemberRepository.GetById(metadata.MemberId.Value),
            x => x.PaymentRepository.GetById(metadata.PaymentId.Value),
            x => x.PaymentCheckoutSessionRepository.GetById(metadata.PaymentCheckoutSessionId.Value));

        // update payment
        if (payment.PaidUtc != null)
        {
            var message =
                $"Not updating Payment {payment.Id} in {paymentProvider} webhook processing: " +
                $"already paid";
            await _loggingService.Warn(message);
        }
        else
        {
            payment.ExternalId = externalId;
            payment.PaidUtc = completedUtc;
            _unitOfWork.PaymentRepository.Update(payment);
        }

        // update payment checkout session
        if (paymentCheckoutSession.CompletedUtc != null)
        {
            var message =
                $"Not updating PaymentCheckoutSession {paymentCheckoutSession.Id} " +
                $"in {paymentProvider} webhook processing: " +
                "already completed";
            await _loggingService.Warn(message);
        }
        else
        {
            paymentCheckoutSession.CompletedUtc = completedUtc;
            _unitOfWork.PaymentCheckoutSessionRepository.Update(paymentCheckoutSession);
        }

        if (metadata.EventTicketPaymentId != null)
        {
            await _unitOfWork.SaveChanges();

            var eventTicketPayment = await _unitOfWork.EventTicketPaymentRepository
                .GetById(metadata.EventTicketPaymentId.Value)
                .Run();

            var @event = await _unitOfWork.EventRepository.GetById(eventTicketPayment.EventId).Run();

            _backgroundTaskService.Enqueue(
                () => _eventService.CompleteEventTicketPurchase(@event.Id, member.Id),
                BackgroundTaskQueueType.Payments);

            var (chapter, currency) = await _unitOfWork.Run(
                /* Default, which ForPlatform reads as no platform filter, so this is a lookup by id alone.
                   Not the payment's own platform: the event already names its chapter, so a platform can only
                   exclude it - and metadata carrying no platform resolves to Drunken Knitwits, which would
                   then miss a Group Squirrel chapter. */
                x => x.ChapterRepository.GetById(PlatformType.Default, @event.ChapterId),
                x => x.CurrencyRepository.GetById(payment.CurrencyId));

            return PaymentWebhookProcessingResult.Successful(
                member, chapter, payment, currency);
        }

        return await UpdateMemberChapterSubscription(
            metadata,
            member,
            payment,
            externalId: externalId,
            completedUtc,
            initiatorId);
    }

    private async Task<PaymentWebhookProcessingResult> ProcessCompletedSiteSubscription(
        PaymentMetadataModel metadata,
        DateTime completedUtc,
        PaymentProviderType paymentProvider,
        string externalId,
        string? initiatorId)
    {
        if (metadata.MemberId == null ||
            metadata.SiteSubscriptionPriceId == null)
        {
            var missingProperties = new[]
            {
                metadata.MemberId == null ? "MemberId" : null,
                metadata.SiteSubscriptionPriceId == null ? "SiteSubscriptionPriceId" : null
            }.Where(x => x != null);

            var message =
                $"Cannot update {paymentProvider} site subscription '{externalId}': " +
                $"metadata missing properties {string.Join(", ", missingProperties)}";

            await _loggingService.Error(message);
            return PaymentWebhookProcessingResult.Failure();
        }

        // Load basic metadata objects
        var (member, siteSubscription, siteSubscriptionPrice, payment, paymentCheckoutSession) = await _unitOfWork.Run(
            x => x.MemberRepository.GetById(metadata.MemberId.Value),
            x => x.SiteSubscriptionRepository.GetByPriceId(metadata.SiteSubscriptionPriceId.Value),
            x => x.SiteSubscriptionPriceRepository.GetById(metadata.SiteSubscriptionPriceId.Value),
            x => metadata.PaymentId != null
                ? x.PaymentRepository.GetByIdOrDefault(metadata.PaymentId.Value)
                : new DefaultDeferredQuerySingleOrDefault<Payment>(),
            x => metadata.PaymentCheckoutSessionId != null
                ? x.PaymentCheckoutSessionRepository.GetByIdOrDefault(metadata.PaymentCheckoutSessionId.Value)
                : new DefaultDeferredQuerySingleOrDefault<PaymentCheckoutSession>());

        // A renewal has no checkout Payment: create one (already paid) and Add it once. Adding then also
        // Upserting would break - Upsert Updates once the Id is set, downgrading the Added state to
        // Modified, so the row is never inserted and the subscription-record FK to it fails.
        if (payment == null)
        {
            payment = _unitOfWork.PaymentRepository.Add(new Payment
            {
                Amount = siteSubscriptionPrice.Amount,
                CreatedUtc = completedUtc,
                CurrencyId = siteSubscriptionPrice.CurrencyId,
                ExternalId = externalId,
                Id = _unitOfWork.NewId(),
                MemberId = member.Id,
                PaidUtc = completedUtc,
                Reference = siteSubscription.ToReference(),
                SitePaymentSettingId = siteSubscription.SitePaymentSettingId
            });
        }
        else if (payment.PaidUtc != null)
        {
            await _loggingService.Warn(
                $"Not updating Payment {payment.Id} in {paymentProvider} webhook processing: already paid");
        }
        else
        {
            payment.ExternalId = externalId;
            payment.PaidUtc = completedUtc;
            _unitOfWork.PaymentRepository.Update(payment);
        }

        // update payment checkout session
        if (paymentCheckoutSession != null)
        {
            if (paymentCheckoutSession.CompletedUtc != null)
            {
                var message =
                    $"Not updating PaymentCheckoutSession {paymentCheckoutSession.Id} in {paymentProvider} webhook processing: " +
                    $"already completed";
                await _loggingService.Warn(message);
            }
            else
            {
                paymentCheckoutSession.CompletedUtc = completedUtc;
                _unitOfWork.PaymentCheckoutSessionRepository.Update(paymentCheckoutSession);
            }
        }

        try
        {
            return await UpdateMemberSiteSubscription(
                member,
                siteSubscriptionPrice,
                payment,
                externalId: externalId,
                completedUtc,
                initiatorId);
        }
        catch (Exception ex)
        {
            await _loggingService.Error("Error processing site subscription webhook", ex);
            throw;
        }
    }

    private async Task<PaymentWebhookProcessingResult> ProcessWebhookCheckoutSessionExpired(PaymentProviderWebhook webhook)
    {
        var utcNow = webhook.OriginatedUtc;

        if (string.IsNullOrEmpty(webhook.PaymentId))
        {
            return PaymentWebhookProcessingResult.Failure();
        }

        if (!webhook.Complete)
        {
            var message =
                $"Received {webhook.PaymentProviderType} webhook '{webhook.Id}' for incomplete event; not processing";

            await _loggingService.Warn(message); ;
            return PaymentWebhookProcessingResult.Failure();
        }

        // Validate basic metadata
        var metadata = PaymentMetadataModel.FromDictionary(webhook.Metadata);

        if (metadata.PaymentCheckoutSessionId == null)
        {
            var message =
                $"Cannot process {webhook.PaymentProviderType} webhook '{webhook.Id}': " +
                $"metadata missing property PaymentCheckoutSessionId";

            await _loggingService.Warn(message);
            return PaymentWebhookProcessingResult.Failure();
        }

        var paymentCheckoutSession = await _unitOfWork.PaymentCheckoutSessionRepository
            .GetById(metadata.PaymentCheckoutSessionId.Value).Run();

        // update payment checkout session
        if (paymentCheckoutSession.ExpiredUtc != null)
        {
            var message =
                $"Not updating PaymentCheckoutSession {paymentCheckoutSession.Id} " +
                $"in {webhook.PaymentProviderType} webhook processing: " +
                $"already expired";
            await _loggingService.Warn(message);

            return PaymentWebhookProcessingResult.Failure();
        }

        paymentCheckoutSession.ExpiredUtc = utcNow;
        _unitOfWork.PaymentCheckoutSessionRepository.Update(paymentCheckoutSession);

        await _unitOfWork.SaveChanges();

        return PaymentWebhookProcessingResult.Successful(member: null, chapter: null, payment: null, currency: null);
    }

    private async Task<PaymentWebhookProcessingResult> ProcessWebhookPayment(PaymentProviderWebhook webhook)
    {
        if (string.IsNullOrEmpty(webhook.PaymentId))
        {
            return PaymentWebhookProcessingResult.Failure();
        }

        if (!webhook.Complete)
        {
            var message =
                $"Received {webhook.PaymentProviderType} webhook '{webhook.Id}' for incomplete event; not processing";

            await _loggingService.Warn(message);
            return PaymentWebhookProcessingResult.Failure();
        }

        return await ProcessCompletedPayment(
            PaymentMetadataModel.FromDictionary(webhook.Metadata),
            webhook.OriginatedUtc,
            webhook.PaymentProviderType,
            webhook.PaymentId,
            initiatorId: webhook.Id);
    }

    private async Task<PaymentWebhookProcessingResult> ProcessWebhookChapterSubscription(
        PaymentProviderWebhook webhook,
        PaymentMetadataModel metadata)
    {
        if (string.IsNullOrEmpty(webhook.SubscriptionId))
        {
            var message =
                $"Cannot process {webhook.PaymentProviderType} webhook '{webhook.Id}': " +
                $"SubscriptionId not set";

            await _loggingService.Error(message);
            return PaymentWebhookProcessingResult.Failure();
        }

        if (webhook.Type == PaymentProviderWebhookType.SubscriptionCancelled)
        {
            await _loggingService.Info(
                $"Processing {webhook.PaymentProviderType} webhook '{webhook.Id}': " +
                $"cancelling member subscription record with external id '{webhook.SubscriptionId}'");

            var memberSubscriptionRecord = await _unitOfWork.MemberSubscriptionRecordRepository
                .Query()
                .ForExternalId(webhook.SubscriptionId)
                .OrderByDescending(x => x.PurchasedUtc)
                .GetSingleOrDefault()
                .Run();
            if (memberSubscriptionRecord == null)
            {
                await _loggingService.Warn(
                    $"No member subscription record found for external id '{webhook.SubscriptionId}'; not cancelling");
                return PaymentWebhookProcessingResult.Failure();
            }

            memberSubscriptionRecord.CancelledUtc = webhook.OriginatedUtc;
            _unitOfWork.MemberSubscriptionRecordRepository.Update(memberSubscriptionRecord);
            await _unitOfWork.SaveChanges();
            return PaymentWebhookProcessingResult.Successful(member: null, chapter: null, payment: null, currency: null);
        }

        return await ProcessCompletedChapterSubscription(
            metadata,
            webhook.OriginatedUtc,
            webhook.PaymentProviderType,
            webhook.SubscriptionId,
            initiatorId: webhook.Id);
    }

    private async Task<PaymentWebhookProcessingResult> ProcessWebhookSiteSubscription(
        PaymentProviderWebhook webhook,
        PaymentMetadataModel metadata)
    {
        if (string.IsNullOrEmpty(webhook.SubscriptionId))
        {
            var message =
                $"Cannot process {webhook.PaymentProviderType} webhook '{webhook.Id}': " +
                $"SubscriptionId not set";

            await _loggingService.Error(message);
            return PaymentWebhookProcessingResult.Failure();
        }

        return await ProcessCompletedSiteSubscription(
            metadata,
            completedUtc: webhook.OriginatedUtc,
            webhook.PaymentProviderType,
            webhook.SubscriptionId,
            initiatorId: webhook.Id);
    }

    /* The platform comes from the webhook's own metadata rather than from the request, because a provider
       posts every platform's events to whichever endpoint was registered with it - one endpoint may serve
       both, and the host it was reached on says nothing about the payment. The checkout that created the
       subscription wrote the platform into the provider's subscription metadata, which is carried by every
       later event on it. */
    private async Task<PaymentWebhookProcessingResult> ProcessWebhookSubscription(
        PaymentProviderWebhook webhook)
    {
        if (string.IsNullOrEmpty(webhook.SubscriptionId))
        {
            var message =
                $"Received {webhook.PaymentProviderType} webhook '{webhook.Id}' for event without SubscriptionId; not processing";

            await _loggingService.Error(message);
            return PaymentWebhookProcessingResult.Failure();
        }

        if (!webhook.Complete)
        {
            var message =
                $"Received {webhook.PaymentProviderType} webhook '{webhook.Id}' for incomplete event; not processing";

            await _loggingService.Warn(message); ;
            return PaymentWebhookProcessingResult.Failure();
        }

        // Validate basic metadata
        var metadata = PaymentMetadataModel.FromDictionary(webhook.Metadata);

        if (metadata.ChapterSubscriptionId != null)
        {
            return await ProcessWebhookChapterSubscription(webhook, metadata);
        }

        if (metadata.SiteSubscriptionPriceId != null)
        {
            return await ProcessWebhookSiteSubscription(webhook, metadata);
        }

        await _loggingService.Error(
            $"Could not process {webhook.PaymentProviderType} webhook '{webhook.Id}': " +
            $"subscription metadata not set");

        return PaymentWebhookProcessingResult.Failure();
    }

    /* Which account a payment was taken through, asked rather than inferred: the account that can produce
       a settlement for the reference is the one holding it, and one that cannot says so plainly. The
       settlement is kept rather than discarded, so proving where the payment lives and reading what it did
       are the same call. Only the payment's own account is asked where it names one; a payment naming none
       is swept against every enabled account, which is what lets one be recorded for it.

       Nothing about the shape of an id is read here. Stripe ids do embed something of the account, but that
       is undocumented and demonstrably unreliable on this data - nine payments already carry references
       from accounts other than the one they name. Asking cannot be wrong in the same way. */
    private async Task<(Guid SitePaymentSettingId, IPaymentProvider Provider, ExternalPaymentSettlement Settlement)?>
        LocatePayment(Payment payment, IReadOnlyCollection<SitePaymentSettings> sitePaymentSettings)
    {
        if (string.IsNullOrEmpty(payment.ExternalId) || payment.PaidUtc == null)
        {
            return null;
        }

        /* Only accounts we are meant to be transacting through are asked. A disabled record is one we are
           not, and in development that is what keeps a live key - which the database holds, being restored
           from production - out of a reconcile run. A payment only a disabled account holds is left alone. */
        var candidates = sitePaymentSettings.Where(x => x.Enabled);

        if (payment.SitePaymentSettingId != null)
        {
            candidates = candidates.Where(x => x.Id == payment.SitePaymentSettingId);
        }

        foreach (var candidate in candidates)
        {
            var paymentProvider = _paymentProviderFactory.GetSitePaymentProviderOrDefault(candidate);
            if (paymentProvider == null)
            {
                continue;
            }

            var externalPaymentId = await paymentProvider.GetPaymentIdForReference(
                payment.ExternalId, payment.PaidUtc.Value);

            if (string.IsNullOrEmpty(externalPaymentId))
            {
                continue;
            }

            var settlement = await paymentProvider.GetPaymentSettlement(externalPaymentId);
            if (settlement != null)
            {
                return (candidate.Id, paymentProvider, settlement);
            }
        }

        return null;
    }

    /* Throws wherever the payment cannot be read, so the job is retried and only one that never becomes
       readable is logged. Used on the webhook path, where the provider has just said the payment exists and
       a failure to read it is therefore a passing one. Reconciling takes the other route - see
       LocatePayment - because there an unreadable payment is a permanent state, not a passing one. */
    private async Task<ExternalPaymentSettlement> ReadPaymentSettlement(
        Payment payment,
        IPaymentProvider paymentProvider,
        string? externalPaymentId,
        string? externalInvoiceId)
    {
        // A recurring subscription's webhook names its invoice and no payment, so the invoice is asked.
        if (string.IsNullOrEmpty(externalPaymentId) && !string.IsNullOrEmpty(externalInvoiceId))
        {
            externalPaymentId = await paymentProvider.GetInvoicePaymentId(externalInvoiceId);
        }

        if (string.IsNullOrEmpty(externalPaymentId))
        {
            throw new OdkServiceException(
                $"Cannot read what Payment {payment.Id} settled: neither the webhook nor invoice " +
                $"'{externalInvoiceId}' names a {paymentProvider.Type} payment");
        }

        var settlement = await paymentProvider.GetPaymentSettlement(externalPaymentId);
        if (settlement == null)
        {
            throw new OdkServiceException(
                $"Could not read {paymentProvider.Type} payment '{externalPaymentId}' for Payment {payment.Id}");
        }

        return settlement;
    }

    private async Task RecordPaymentSettlement(
        Payment payment,
        IPaymentProvider paymentProvider,
        ChapterPaymentAccount? connectedAccount,
        ExternalPaymentSettlement settlement)
    {
        if (settlement.NetAmount == null)
        {
            throw new OdkServiceException(
                $"{paymentProvider.Type} charge '{settlement.ChargeId}' for Payment {payment.Id} " +
                $"has not settled yet");
        }

        var netAmount = settlement.NetAmount.Value;

        decimal? connectedAccountAmount;

        if (settlement.CollectedCommissionAmount != null)
        {
            /* A charge the provider split and transferred itself, taken before that became ours to do. What
               happened to it is a matter of record: the group was left with the amount less the commission
               the provider collected, and we were left with the rest of the net. Recomputing it from the
               current rate would state figures that never occurred. */
            connectedAccountAmount = settlement.Amount - settlement.CollectedCommissionAmount;
            payment.TransferredUtc = settlement.TransferredUtc;
        }
        else if (connectedAccount != null)
        {
            /* The commission comes out of the net, so the provider's fee is met before we take a cut - which
               is why it cannot be applied when the charge is made, the fee not being knowable until the
               member has chosen a card. */
            var commissionAmount = Math.Round(
                netAmount * paymentProvider.CommissionPercentage / 100, 2, MidpointRounding.AwayFromZero);
            connectedAccountAmount = netAmount - commissionAmount;
        }
        else
        {
            connectedAccountAmount = null;
        }

        payment.ActualAmount = settlement.Amount;
        payment.ActualCommissionAmount = connectedAccountAmount != null
            ? netAmount - connectedAccountAmount
            : null;
        payment.ActualConnectedAccountAmount = connectedAccountAmount;
        payment.ActualFeeAmount = settlement.FeeAmount;
        payment.ActualNetAmount = netAmount;
        payment.ExternalChargeId = settlement.ChargeId;
        payment.SettlementCurrencyCode = settlement.SettlementCurrencyCode;

        _unitOfWork.PaymentRepository.Update(payment);
        await _unitOfWork.SaveChanges();
    }

    private async Task ResolvePaymentSettlement(
        Guid paymentId, string? externalPaymentId, string? externalInvoiceId)
    {
        var (payment, sitePaymentSettings) = await _unitOfWork.Run(
            x => x.PaymentRepository.GetById(paymentId),
            x => x.SitePaymentSettingsRepository.GetAll());

        var paymentProvider = _paymentProviderFactory.GetSitePaymentProvider(
            sitePaymentSettings, payment.SitePaymentSettingId);

        /* What decides whether anything is owed onward is a connected account to owe it to, not the payment
           belonging to a group: a group that has never finished setting up payments has no account, and
           there is then nothing to transfer and no commission to take. */
        var connectedAccount = payment.ChapterId != null
            ? await _unitOfWork.ChapterPaymentAccountRepository
                .Query()
                .ForChapter(payment.ChapterId.Value)
                .GetSingleOrDefault()
                .Run()
            : null;

        // Already read, and what moved cannot move differently the second time; only the transfer is left.
        if (payment.ActualAmount == null)
        {
            ExternalPaymentSettlement settlement;

            /* Reconciling, rather than acting on a webhook: no ids were handed in, so the reference
               recorded when the payment was taken is all there is to go on, and the account holding it has
               to be found before anything can be asked about it. */
            if (string.IsNullOrEmpty(externalPaymentId) && string.IsNullOrEmpty(externalInvoiceId))
            {
                var located = await LocatePayment(payment, sitePaymentSettings);

                if (located == null)
                {
                    /* Skipped rather than thrown: no configured account holds it, and no number of retries
                       will change which account does. A warning rather than an error, because a payment
                       taken through an account no longer configured is a fact about the data, not a fault. */
                    await _loggingService.Warn(
                        $"Not reconciling Payment {payment.Id}: no configured account holds " +
                        $"'{payment.ExternalId}'");
                    return;
                }

                (var sitePaymentSettingId, paymentProvider, settlement) = located.Value;

                if (payment.SitePaymentSettingId != sitePaymentSettingId)
                {
                    /* Recorded because it has been proven, not guessed: the account that produced a
                       settlement for the reference is the one the payment was taken through. */
                    payment.SitePaymentSettingId = sitePaymentSettingId;
                }
            }
            else
            {
                settlement = await ReadPaymentSettlement(
                    payment, paymentProvider, externalPaymentId, externalInvoiceId);
            }

            await RecordPaymentSettlement(payment, paymentProvider, connectedAccount, settlement);
        }

        if (connectedAccount != null)
        {
            await TransferConnectedAccountShare(payment, paymentProvider, connectedAccount);
        }
    }

    /* The group's share, moved once the settlement says what there is to share. Idempotent twice over: the
       date below stops a second attempt starting, and the provider is given a key derived from the payment,
       so an attempt that moved the money but failed before recording it cannot move it again. */
    private async Task TransferConnectedAccountShare(
        Payment payment, IPaymentProvider paymentProvider, ChapterPaymentAccount connectedAccount)
    {
        if (payment.ActualConnectedAccountAmount == null || payment.TransferredUtc != null)
        {
            return;
        }

        var currency = await _unitOfWork.CurrencyRepository.GetById(payment.CurrencyId).Run();

        /* The share was worked out from a net stated in the settlement currency, so paying it in the
           currency charged would silently pay the wrong amount whenever the two differ. */
        if (!string.Equals(payment.SettlementCurrencyCode, currency.Code, StringComparison.OrdinalIgnoreCase))
        {
            throw new OdkServiceException(
                $"Cannot transfer the group's share of Payment {payment.Id}: it settled in " +
                $"'{payment.SettlementCurrencyCode}' but is denominated in '{currency.Code}'");
        }

        var result = await paymentProvider.CreateTransfer(new ExternalTransfer
        {
            Amount = payment.ActualConnectedAccountAmount.Value,
            ConnectedAccountId = connectedAccount.ExternalId,
            CurrencyCode = currency.Code,
            ExternalChargeId = payment.ExternalChargeId ?? string.Empty,
            IdempotencyKey = ToTransferIdempotencyKey(payment.Id)
        });

        if (!result.Success)
        {
            throw new OdkServiceException(
                $"Could not transfer {payment.ActualConnectedAccountAmount} {currency.Code} to the group " +
                $"for Payment {payment.Id}: {result.Message}");
        }

        payment.TransferredUtc = DateTime.UtcNow;
        _unitOfWork.PaymentRepository.Update(payment);
        await _unitOfWork.SaveChanges();
    }

    private async Task<PaymentWebhookProcessingResult> UpdateMemberChapterSubscription(
        PaymentMetadataModel metadata,
        Member member,
        Payment payment,
        string externalId,
        DateTime utcNow,
        string? initiatorId)
    {
        if (metadata.ChapterId == null || metadata.ChapterSubscriptionId == null)
        {
            var message =
                $"ChapterId or ChapterSubscriptionId not on payment metadata; " +
                $"not updating member chapter subscription";
            await _loggingService.Warn(message);
            return PaymentWebhookProcessingResult.Failure();
        }

        var platform = metadata.PlatformOrDrunkenKnitwits;

        var (chapter, chapterSubscription) = await _unitOfWork.Run(
            x => x.ChapterRepository.GetById(platform, metadata.ChapterId.Value),
            x => x.ChapterSubscriptionRepository.GetById(metadata.ChapterSubscriptionId.Value));

        if (chapter.Id != chapterSubscription.ChapterId)
        {
            var message =
                $"Chapter subscription {chapterSubscription.Id} not for chapter {chapter.Id}; " +
                $"not updating member chapter subscription";
            await _loggingService.Warn(message);
            return PaymentWebhookProcessingResult.Failure();
        }

        var memberChapter = member.MemberChapter(chapterSubscription.ChapterId);
        if (memberChapter == null)
        {
            var message =
                $"Member {member.Id} not in chapter {chapter.Id}; " +
                $"not updating member chapter subscription";
            await _loggingService.Warn(message);
            return PaymentWebhookProcessingResult.Failure();
        }

        var (chapterId, memberId) = (chapter.Id, member.Id);

        var (currentRecord, recordForInitiator, membershipSettings) = await _unitOfWork.Run(
            x => x.MemberSubscriptionRecordRepository.Query().Current().ForMember(memberId).ForChapter(chapterId).GetSingleOrDefault(),
            x => !string.IsNullOrEmpty(initiatorId)
                ? x.MemberSubscriptionRecordRepository
                    .Query()
                    .ForInitiator(initiatorId)
                    .GetSingleOrDefault()
                : new DefaultDeferredQuerySingleOrDefault<MemberSubscriptionRecord>(),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapterId));

        // Idempotency: if this initiating event (the payment provider webhook id) has already recorded a
        // subscription, do not record it again. This protects against a retry of the webhook-processing job
        // re-applying the same event. A genuine renewal carries a distinct webhook id, so it is not caught
        // here - it appends a new current record below (the unique InitiatorId index is the final backstop).
        if (recordForInitiator != null)
        {
            await _loggingService.Info(
                $"Chapter subscription already updated for initiator '{initiatorId}'; not updating again");
            return PaymentWebhookProcessingResult.Successful(
                member, chapter, payment, chapterSubscription.Currency);
        }

        // A recurring subscription expires when the provider next takes payment, so the two cannot drift
        // apart: the provider anchors its schedule to the original purchase
        var nextPaymentUtc = chapterSubscription.Recurring
            ? await GetChapterSubscriptionNextPaymentDate(chapterId, externalId)
            : null;

        // A negative cooldown is meaningless and is treated as none, so it cannot narrow the window to less
        // than a live period.
        var cooldownDays = Math.Max(0, membershipSettings?.MembershipDisabledAfterDaysExpired ?? 0);

        // A one-off has no schedule to read, so its expiry is calculated from the current log record (the
        // source of truth). The value is fixed on the new record at insert.
        var expiresUtc = nextPaymentUtc ?? RollExpiryForward(
            currentRecord?.ExpiresUtc,
            chapterSubscription.Months,
            cooldownStartUtc: utcNow.AddDays(-cooldownDays),
            utcNow);

        // Append a new current record for this payment (renewals keep the subscription's history).
        _memberChapterSubscriptionWriter.MakeRecordCurrent(
            newRecord: new MemberSubscriptionRecord
            {
                Amount = chapterSubscription.Amount,
                ChapterId = chapterId,
                ChapterSubscriptionId = chapterSubscription.Id,
                ExpiresUtc = expiresUtc,
                // no need to store external id for one-off purchases
                ExternalId = chapterSubscription.Recurring ? externalId : null,
                InitiatorId = initiatorId,
                MemberId = memberId,
                Months = chapterSubscription.Months,
                PaymentId = payment.Id,
                PurchasedUtc = utcNow,
                Type = chapterSubscription.Type
            },
            existingCurrent: currentRecord);

        var previousExpiresUtc = currentRecord?.ExpiresUtc?.ToString("yyyy-MM-dd HH:mm:ss") ?? "none";
        await _loggingService.Info(
            $"Updating member {member.Id} subscription for chapter {chapter.Name}. " +
            $"Updating expiry date from {previousExpiresUtc} to {expiresUtc:yyyy-MM-dd HH:mm:ss}");

        await _unitOfWork.SaveChanges();

        return PaymentWebhookProcessingResult.Successful(
            member, chapter, payment, chapterSubscription.Currency);
    }

    private async Task<PaymentWebhookProcessingResult> UpdateMemberSiteSubscription(
        Member member,
        SiteSubscriptionPrice siteSubscriptionPrice,
        Payment payment,
        string externalId,
        DateTime utcNow,
        string? initiatorId)
    {
        var memberId = member.Id;

        var (recordForInitiator, currentRecord, sitePaymentSettings) = await _unitOfWork.Run(
            x => !string.IsNullOrEmpty(initiatorId)
                ? x.MemberSiteSubscriptionRecordRepository.Query().ForInitiator(initiatorId).GetSingleOrDefault()
                : new DefaultDeferredQuerySingleOrDefault<MemberSiteSubscriptionRecord>(),
            x => x.MemberSiteSubscriptionRecordRepository.Query().Current().ForMember(memberId).GetSingleOrDefault(),
            x => x.SitePaymentSettingsRepository.GetAll());

        // Idempotency: if this initiating event (the payment provider webhook id) has already extended a
        // subscription, do not extend again. This protects against a retry of the webhook-processing job
        // re-applying the same event after the extension has already been committed. Renewals carry a
        // distinct webhook id, so they are not caught here. Keying on the payment id would instead skip
        // renewals, since recurring invoices reuse the original checkout Payment.
        if (recordForInitiator != null)
        {
            await _loggingService.Info(
                $"Site subscription already updated for initiator '{initiatorId}'; not updating again");
            return PaymentWebhookProcessingResult.Successful(
                member, chapter: null, payment, siteSubscriptionPrice.Currency);
        }

        // A site subscription is always a provider subscription, so it expires when payment is next taken.
        var paymentProvider = _paymentProviderFactory.GetSitePaymentProvider(
            sitePaymentSettings, payment.SitePaymentSettingId);
        var nextPaymentUtc = await GetNextPaymentDate(paymentProvider, externalId);

        var months = siteSubscriptionPrice.Frequency.Months();

        // Where the provider has no date to give, roll the expiry forward from the current record's expiry
        // (the log is the source of truth); the value is fixed on the new record at insert.
        var expiresUtc = nextPaymentUtc ?? RollExpiryForward(
            currentRecord?.ExpiresUtc,
            months,
            _siteSubscriptionCooldown.ActiveAfterUtc(utcNow),
            utcNow);

        // Append a new current record for this payment (renewals keep the subscription's history).
        _memberSiteSubscriptionWriter.MakeRecordCurrent(
            newRecord: new MemberSiteSubscriptionRecord
            {
                CreatedUtc = utcNow,
                ExpiresUtc = expiresUtc,
                ExternalId = externalId,
                InitiatorId = initiatorId,
                MemberId = memberId,
                PaymentId = payment.Id,
                SiteSubscriptionId = siteSubscriptionPrice.SiteSubscriptionId,
                SiteSubscriptionPriceId = siteSubscriptionPrice.Id
            },
            existingCurrent: currentRecord);

        await _unitOfWork.SaveChanges();

        return PaymentWebhookProcessingResult.Successful(
            member, chapter: null, payment, siteSubscriptionPrice.Currency);
    }
}