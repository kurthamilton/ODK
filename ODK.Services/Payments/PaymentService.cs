using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Events;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.Payments.Models;
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
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(
        IUnitOfWork unitOfWork,
        ILoggingService loggingService,
        IMemberEmailService memberEmailService,
        IPaymentProviderFactory paymentProviderFactory,
        IEventService eventService,
        IBackgroundTaskService backgroundTaskService,
        IMemberChapterSubscriptionWriter memberChapterSubscriptionWriter,
        IMemberSiteSubscriptionWriter memberSiteSubscriptionWriter)
    {
        _backgroundTaskService = backgroundTaskService;
        _eventService = eventService;
        _loggingService = loggingService;
        _memberChapterSubscriptionWriter = memberChapterSubscriptionWriter;
        _memberEmailService = memberEmailService;
        _memberSiteSubscriptionWriter = memberSiteSubscriptionWriter;
        _paymentProviderFactory = paymentProviderFactory;
        _unitOfWork = unitOfWork;
    }

    public async Task EnsureProductExists(IChapterServiceRequest request)
    {
        var chapter = request.Chapter;

        var (chapterPaymentSettings, sitePaymentSettings) = await _unitOfWork.RunAsync(
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapter.Id),
            x => x.SitePaymentSettingsRepository.GetActive());

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

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PaymentStatusType> GetMemberChapterPaymentCheckoutSessionStatus(
        IMemberServiceRequest request, Guid chapterId, string externalSessionId)
    {
        var (sitePaymentSettings, paymentAccount, checkoutSession) = await _unitOfWork.RunAsync(
            x => x.SitePaymentSettingsRepository.GetActive(),
            x => x.ChapterPaymentAccountRepository.GetByChapterId(chapterId),
            x => x.PaymentCheckoutSessionRepository.GetByMemberId(request.CurrentMember.Id, externalSessionId));

        OdkAssertions.Exists(checkoutSession);

        if (checkoutSession.CompletedUtc != null)
        {
            return PaymentStatusType.Complete;
        }

        var paymentProvider = _paymentProviderFactory.GetPaymentProvider(
            sitePaymentSettings,
            paymentAccount);

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
        var (checkoutSession, sitePaymentSettings) = await _unitOfWork.RunAsync(
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

    // Public for Hangfire. This parameterless-initiator overload preserves the method signature for jobs
    // that were enqueued before initiatorId was introduced, so a deployment can't orphan in-flight jobs.
    public Task<PaymentWebhookProcessingResult> ProcessCompletedChapterSubscription(
        PlatformType platform,
        IReadOnlyDictionary<string, string> metadataDictionary,
        DateTime completedUtc,
        PaymentProviderType paymentProvider,
        string externalId)
        => ProcessCompletedChapterSubscription(
            platform, metadataDictionary, completedUtc, paymentProvider, externalId, initiatorId: null);

    // Public for Hangfire. This parameterless-initiator overload preserves the method signature for jobs
    // that were enqueued before initiatorId was introduced, so a deployment can't orphan in-flight jobs.
    public Task<PaymentWebhookProcessingResult> ProcessCompletedPayment(
        IReadOnlyDictionary<string, string> metadataDictionary,
        DateTime completedUtc,
        PaymentProviderType paymentProvider,
        string externalId)
        => ProcessCompletedPayment(
            metadataDictionary, completedUtc, paymentProvider, externalId, initiatorId: null);

    // Public for Hangfire. This parameterless-initiator overload preserves the method signature for jobs
    // that were enqueued before initiatorId was introduced, so a deployment can't orphan in-flight jobs.
    public Task<PaymentWebhookProcessingResult> ProcessCompletedSiteSubscription(
        IReadOnlyDictionary<string, string> metadataDictionary,
        DateTime completedUtc,
        PaymentProviderType paymentProvider,
        string externalId)
        => ProcessCompletedSiteSubscription(
            metadataDictionary, completedUtc, paymentProvider, externalId, initiatorId: null);

    public async Task ProcessWebhook(IServiceRequest request, PaymentProviderWebhook webhook)
    {
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

        await _unitOfWork.SaveChangesAsync();

        // Run the actioning of the webhook itself in a new task so that we can persist the event as quickly as possible
        // and make the actual processing retryable.
        _backgroundTaskService.Enqueue(
            () => ProcessWebhookAction(request, webhook),
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
                result = await ProcessWebhookSubscription(request.Platform, webhook);
                break;

            default:
                result = PaymentWebhookProcessingResult.Failure();
                break;
        }

        if (!result.Success)
        {
            return;
        }

        if (result.Payment != null &&
            result.Currency != null &&
            result.Member != null)
        {
            var (member, chapter, currency, payment) = (result.Member, result.Chapter, result.Currency, result.Payment);

            await _memberEmailService.SendPaymentNotification(request, member, chapter, payment, currency);
        }
    }

    // A period that is still live - or lapsed but inside the chapter's cooldown - is continued, so a
    // membership keeps its anniversary instead of drifting by however late the member renewed. Otherwise the
    // period starts now. A cooldown of zero therefore continues only a live period.
    //
    // A cooldown longer than the subscription's own length can continue a period that has already fully
    // elapsed, so a calculated expiry that is not in the future starts a new period instead: a payment must
    // always leave the member current.
    private static DateTime RollExpiryForward(
        DateTime? currentExpiresUtc,
        int months,
        int cooldownDaysAfterExpiry,
        DateTime utcNow)
    {
        // A negative cooldown is meaningless and is treated as none, so it cannot narrow the window to less
        // than a live period.
        var cooldownStartUtc = utcNow.AddDays(-Math.Max(0, cooldownDaysAfterExpiry));

        var continueFromUtc = currentExpiresUtc >= cooldownStartUtc
            ? currentExpiresUtc.Value
            : utcNow;

        var expiresUtc = continueFromUtc.AddMonths(months);

        return expiresUtc > utcNow
            ? expiresUtc
            : utcNow.AddMonths(months);
    }

    private async Task<DateTime?> GetChapterSubscriptionNextPaymentDate(Guid chapterId, string externalId)
    {
        var (sitePaymentSettings, paymentAccount) = await _unitOfWork.RunAsync(
            x => x.SitePaymentSettingsRepository.GetActive(),
            x => x.ChapterPaymentAccountRepository.GetByChapterId(chapterId));

        var paymentProvider = _paymentProviderFactory.GetPaymentProvider(sitePaymentSettings, paymentAccount);

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
        PlatformType platform,
        IReadOnlyDictionary<string, string> metadataDictionary,
        DateTime completedUtc,
        PaymentProviderType paymentProvider,
        string externalId,
        string? initiatorId)
    {
        var metadata = PaymentMetadataModel.FromDictionary(metadataDictionary);

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
        var (member, chapter, chapterSubscription, payment, paymentCheckoutSession) = await _unitOfWork.RunAsync(
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
                Id = Guid.NewGuid(),
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
        IReadOnlyDictionary<string, string> metadataDictionary,
        DateTime completedUtc,
        PaymentProviderType paymentProvider,
        string externalId,
        string? initiatorId)
    {
        // Validate basic metadata
        var metadata = PaymentMetadataModel.FromDictionary(metadataDictionary);

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
        var (member, payment, paymentCheckoutSession) = await _unitOfWork.RunAsync(
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
            await _unitOfWork.SaveChangesAsync();

            var eventTicketPayment = await _unitOfWork.EventTicketPaymentRepository
                .GetById(metadata.EventTicketPaymentId.Value)
                .Run();

            var @event = await _unitOfWork.EventRepository.GetById(eventTicketPayment.EventId).Run();

            _backgroundTaskService.Enqueue(
                () => _eventService.CompleteEventTicketPurchase(@event.Id, member.Id),
                BackgroundTaskQueueType.Payments);

            var (chapter, currency) = await _unitOfWork.RunAsync(
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
        IReadOnlyDictionary<string, string> metadataDictionary,
        DateTime completedUtc,
        PaymentProviderType paymentProvider,
        string externalId,
        string? initiatorId)
    {
        var metadata = PaymentMetadataModel.FromDictionary(metadataDictionary);

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
        var (member, siteSubscription, siteSubscriptionPrice, payment, paymentCheckoutSession) = await _unitOfWork.RunAsync(
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
                Id = Guid.NewGuid(),
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

        await _unitOfWork.SaveChangesAsync();

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
            webhook.Metadata,
            webhook.OriginatedUtc,
            webhook.PaymentProviderType,
            webhook.PaymentId,
            initiatorId: webhook.Id);
    }

    private async Task<PaymentWebhookProcessingResult> ProcessWebhookChapterSubscription(
        PlatformType platform,
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
            await _unitOfWork.SaveChangesAsync();
            return PaymentWebhookProcessingResult.Successful(member: null, chapter: null, payment: null, currency: null);
        }

        return await ProcessCompletedChapterSubscription(
            platform,
            metadata.ToDictionary(),
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
            metadata.ToDictionary(),
            completedUtc: webhook.OriginatedUtc,
            webhook.PaymentProviderType,
            webhook.SubscriptionId,
            initiatorId: webhook.Id);
    }

    private async Task<PaymentWebhookProcessingResult> ProcessWebhookSubscription(
        PlatformType platform,
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
            return await ProcessWebhookChapterSubscription(platform, webhook, metadata);
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

        var platform = metadata.Platform ?? PlatformType.DrunkenKnitwits;

        var (chapter, chapterSubscription) = await _unitOfWork.RunAsync(
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

        var (currentRecord, recordForInitiator, membershipSettings) = await _unitOfWork.RunAsync(
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

        // A one-off has no schedule to read, so its expiry is calculated from the current log record (the
        // source of truth). The value is fixed on the new record at insert.
        var expiresUtc = nextPaymentUtc ?? RollExpiryForward(
            currentRecord?.ExpiresUtc,
            chapterSubscription.Months,
            membershipSettings?.MembershipDisabledAfterDaysExpired ?? 0,
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

        await _unitOfWork.SaveChangesAsync();

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

        var (recordForInitiator, currentRecord, sitePaymentSettings) = await _unitOfWork.RunAsync(
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
        var expiresUtc = nextPaymentUtc
            ?? (currentRecord?.ExpiresUtc > utcNow
                ? currentRecord.ExpiresUtc.Value.AddMonths(months)
                : utcNow.AddMonths(months));

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

        await _unitOfWork.SaveChangesAsync();

        return PaymentWebhookProcessingResult.Successful(
            member, chapter: null, payment, siteSubscriptionPrice.Currency);
    }
}