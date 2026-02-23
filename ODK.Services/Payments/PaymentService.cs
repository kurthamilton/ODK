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
using ODK.Services.Tasks;

namespace ODK.Services.Payments;

public class PaymentService : IPaymentService
{
    private readonly IBackgroundTaskService _backgroundTaskService;
    private readonly IEventService _eventService;
    private readonly ILoggingService _loggingService;
    private readonly IMemberEmailService _memberEmailService;
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(
        IUnitOfWork unitOfWork,
        ILoggingService loggingService,
        IMemberEmailService memberEmailService,
        IPaymentProviderFactory paymentProviderFactory,
        IEventService eventService,
        IBackgroundTaskService backgroundTaskService)
    {
        _backgroundTaskService = backgroundTaskService;
        _eventService = eventService;
        _loggingService = loggingService;
        _memberEmailService = memberEmailService;
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

        var payment = await _unitOfWork.PaymentRepository.GetById(checkoutSession.PaymentId).Run();

        var paymentProvider = _paymentProviderFactory.GetPaymentProvider(
            sitePaymentSettings,
            paymentAccount);

        var externalSession = await paymentProvider.GetCheckoutSession(externalSessionId);
        if (externalSession == null)
        {
            return PaymentStatusType.Expired;
        }

        if (externalSession.CompletedUtc != null)
        {
            if (externalSession.SubscriptionId != null)
            {
                // Enqueue the chapter subscription processing to avoid race conditions with webhooks
                // Payment status Completed will be returned on the next request after the subscription has been processed
                _backgroundTaskService.Enqueue(
                    () => ProcessCompletedChapterSubscription(
                        externalSession.Metadata,
                        externalSession.CompletedUtc.Value,
                        paymentProvider.Type,
                        externalSession.SubscriptionId),
                    BackgroundTaskQueueType.Payments);
            }
            else if (externalSession.PaymentId != null)
            {
                // Enqueue the payment processing to avoid race conditions with webhooks
                // Payment status Completed will be returned on the next request after the payment has been processed
                _backgroundTaskService.Enqueue(
                    () => ProcessCompletedPayment(
                        externalSession.Metadata,
                        externalSession.CompletedUtc.Value,
                        paymentProvider.Type,
                        externalSession.PaymentId),
                    BackgroundTaskQueueType.Payments);
            }
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

        var externalSession = await paymentProvider.GetCheckoutSession(externalSessionId);        

        if (externalSession == null)
        {
            return PaymentStatusType.Expired;
        }

        if (externalSession.SubscriptionId != null &&
            externalSession.CompletedUtc != null)
        {
            // Enqueue the site subscription processing to avoid race conditions with webhooks
            // Payment status Completed will be returned on the next request after the subscription has been processed
            _backgroundTaskService.Enqueue(
                () => ProcessCompletedSiteSubscription(
                    externalSession.Metadata,
                    externalSession.CompletedUtc.Value,
                    paymentProvider.Type,
                    externalSession.SubscriptionId),
                BackgroundTaskQueueType.Payments);
        }        

        return PaymentStatusType.Pending;
    }

    // Public for Hangfire
    public async Task<PaymentWebhookProcessingResult> ProcessCompletedChapterSubscription(
        IReadOnlyDictionary<string, string> metadataDictionary,
        DateTime completedUtc,
        PaymentProviderType paymentProvider,
        string externalId)
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
            x => x.ChapterSubscriptionRepository.GetByIdOrDefault(metadata.ChapterSubscriptionId.Value),
            x => x.ChapterSubscriptionRepository.GetById(metadata.ChapterSubscriptionId.Value),
            x => metadata.PaymentId != null
                ? x.PaymentRepository.GetByIdOrDefault(metadata.PaymentId.Value)
                : new DefaultDeferredQuerySingleOrDefault<Payment>(),
            x => metadata.PaymentCheckoutSessionId != null
                ? x.PaymentCheckoutSessionRepository.GetByIdOrDefault(metadata.PaymentCheckoutSessionId.Value)
                : new DefaultDeferredQuerySingleOrDefault<PaymentCheckoutSession>());

        if (payment == null)
        {
            // The first payment for a subscription will be preceded by the creation of a Payment at checkout.
            // We will need to create a Payment here for recurring payments.
            payment = new Payment
            {
                Amount = (decimal)chapterSubscription.Amount,
                ChapterId = chapter.Id,
                CreatedUtc = completedUtc,
                CurrencyId = chapterSubscription.CurrencyId,
                Id = Guid.NewGuid(),
                MemberId = member.Id,
                Reference = chapterSubscription.ToReference(),
                SitePaymentSettingId = chapterSubscription.SitePaymentSettingId
            };
        }

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
            _unitOfWork.PaymentRepository.Upsert(payment);
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
            completedUtc);
    }

    // Public for Hangfire
    public async Task<PaymentWebhookProcessingResult> ProcessCompletedPayment(
        IReadOnlyDictionary<string, string> metadataDictionary,
        DateTime completedUtc,
        PaymentProviderType paymentProvider,
        string externalId)
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
            completedUtc);
    }

    // Public for Hangfire
    public async Task<PaymentWebhookProcessingResult> ProcessCompletedSiteSubscription(
        IReadOnlyDictionary<string, string> metadataDictionary, 
        DateTime completedUtc, 
        PaymentProviderType paymentProvider, 
        string externalId)
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

        if (payment == null)
        {
            // The first payment for a subscription will be preceded by the creation of a Payment at checkout.
            // We will need to create a Payment here for recurring payments.
            payment = new Payment
            {
                Amount = siteSubscriptionPrice.Amount,
                CreatedUtc = completedUtc,
                CurrencyId = siteSubscriptionPrice.CurrencyId,
                Id = Guid.NewGuid(),
                MemberId = member.Id,
                Reference = siteSubscription.ToReference(),
                SitePaymentSettingId = siteSubscription.SitePaymentSettingId
            };
        }

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
            _unitOfWork.PaymentRepository.Upsert(payment);
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
                metadata.Platform ?? PlatformType.DrunkenKnitwits,
                member,
                siteSubscriptionPrice,
                payment,
                externalId: externalId,
                completedUtc);
        }
        catch (Exception ex)
        {
            await _loggingService.Error("Error processing site subscription webhook", ex);
            throw;
        }
    }

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

        PaymentWebhookProcessingResult result;

        switch (webhook.Type)
        {
            case PaymentProviderWebhookType.CheckoutSessionCompleted:
            case PaymentProviderWebhookType.PaymentSucceeded:
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

        await _unitOfWork.SaveChangesAsync();

        if (!result.Success)
        {
            return;
        }

        if (result.Payment != null &&
            result.Currency != null &&
            result.Member != null)
        {
            var metadata = PaymentMetadataModel.FromDictionary(webhook.Metadata);
            var platform = metadata.Platform ?? PlatformType.DrunkenKnitwits;

            var siteEmailSettings = await _unitOfWork.SiteEmailSettingsRepository
                .Get(platform)
                .Run();

            var (member, chapter, currency, payment) = (result.Member, result.Chapter, result.Currency, result.Payment);

            await _memberEmailService.SendPaymentNotification(request, member, chapter, payment, currency, siteEmailSettings);
        }
    }    

    private async Task<PaymentWebhookProcessingResult> ProcessWebhookCheckoutSessionExpired(PaymentProviderWebhook webhook)
    {
        var utcNow = webhook.OriginatedUtc;

        if (string.IsNullOrEmpty(webhook.PaymentId))
        {
            var message =
                $"Received {webhook.PaymentProviderType} webhook '{webhook.Id}' for event without PaymentId; not processing";

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
            var message =
                $"Received {webhook.PaymentProviderType} webhook '{webhook.Id}' for event without PaymentId; not processing";

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

        return await ProcessCompletedPayment(
            webhook.Metadata,
            webhook.OriginatedUtc,
            webhook.PaymentProviderType,
            webhook.PaymentId);
    }

    private async Task<PaymentWebhookProcessingResult> ProcessWebhookChapterSubscription(
        PaymentProviderWebhook webhook,
        PaymentMetadataModel metadata)
    {
        var utcNow = webhook.OriginatedUtc;

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
                .GetByExternalId(webhook.SubscriptionId).Run();

            memberSubscriptionRecord.CancelledUtc = webhook.OriginatedUtc;
            _unitOfWork.MemberSubscriptionRecordRepository.Update(memberSubscriptionRecord);
            await _unitOfWork.SaveChangesAsync();
            return PaymentWebhookProcessingResult.Successful(member: null, chapter: null, payment: null, currency: null);
        }

        return await ProcessCompletedChapterSubscription(
            metadata.ToDictionary(),
            webhook.OriginatedUtc,
            webhook.PaymentProviderType,
            webhook.SubscriptionId);
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
            webhook.SubscriptionId);
    }

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

    private async Task<PaymentWebhookProcessingResult> UpdateMemberChapterSubscription(
        PaymentMetadataModel metadata,
        Member member,
        Payment payment,
        string externalId,
        DateTime utcNow)
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

        var (memberSubscription, existingMemberSubscriptionRecord) = await _unitOfWork.RunAsync(
            x => x.MemberSubscriptionRepository.GetByMemberId(memberId, chapterId),
            x => x.MemberSubscriptionRecordRepository.GetByExternalIdOrDefault(externalId));

        if (existingMemberSubscriptionRecord != null)
        {
            var message =
                $"Member subscription record already exists for externalId '{externalId}'; " +
                $"not updating member chapter subscription";
            await _loggingService.Warn(message);
            return PaymentWebhookProcessingResult.Failure();
        }

        memberSubscription ??= new();

        var expiresUtc = memberSubscription.ExpiresUtc > utcNow
            ? memberSubscription.ExpiresUtc.Value
            : utcNow;
        expiresUtc = expiresUtc.AddMonths(chapterSubscription.Months);
        memberSubscription.ExpiresUtc = expiresUtc;
        memberSubscription.Type = chapterSubscription.Type;

        if (memberSubscription.MemberChapterId == default)
        {
            memberSubscription.MemberChapterId = memberChapter.Id;
            _unitOfWork.MemberSubscriptionRepository.Add(memberSubscription);
        }
        else
        {
            _unitOfWork.MemberSubscriptionRepository.Update(memberSubscription);
        }

        _unitOfWork.MemberSubscriptionRecordRepository.Add(new MemberSubscriptionRecord
        {
            Amount = chapterSubscription.Amount,
            ChapterId = chapterId,
            ChapterSubscriptionId = chapterSubscription.Id,
            ExternalId = externalId,
            MemberId = memberId,
            Months = chapterSubscription.Months,
            PaymentId = payment.Id,
            PurchasedUtc = utcNow,
            Type = chapterSubscription.Type
        });

        await _unitOfWork.SaveChangesAsync();

        return PaymentWebhookProcessingResult.Successful(
            member, chapter, payment, chapterSubscription.Currency);
    }

    private async Task<PaymentWebhookProcessingResult> UpdateMemberSiteSubscription(
        PlatformType platform,
        Member member,
        SiteSubscriptionPrice siteSubscriptionPrice,
        Payment payment,
        string externalId,
        DateTime utcNow)
    {
        var memberId = member.Id;

        var (recordExists, memberSubscription) = await _unitOfWork.RunAsync(
            x => x.MemberSiteSubscriptionRecordRepository.Query().ForPayment(payment.Id).Any(),
            x => x.MemberSiteSubscriptionRepository.GetByMemberId(memberId, platform));

        if (recordExists)
        {
            await _loggingService.Warn(
                $"Member site subscription record already exists for payment {payment.Id}: " +
                $"not updating member site subscription");
            return PaymentWebhookProcessingResult.Successful(
                member, chapter: null, payment, siteSubscriptionPrice.Currency);
        }

        memberSubscription ??= new MemberSiteSubscription();

        var months = siteSubscriptionPrice.Frequency == SiteSubscriptionFrequency.Yearly
            ? 12
            : 1;

        var expiresUtc = memberSubscription.ExpiresUtc > utcNow
            ? memberSubscription.ExpiresUtc.Value.AddMonths(months)
            : utcNow.AddMonths(months);

        memberSubscription.ExpiresUtc = expiresUtc;
        memberSubscription.ExternalId = externalId;
        memberSubscription.SiteSubscriptionPriceId = siteSubscriptionPrice.Id;
        memberSubscription.SiteSubscriptionId = siteSubscriptionPrice.SiteSubscriptionId;

        if (memberSubscription.Id == default)
        {
            memberSubscription.Id = Guid.NewGuid();
            memberSubscription.MemberId = memberId;
            _unitOfWork.MemberSiteSubscriptionRepository.Add(memberSubscription);
        }
        else
        {
            _unitOfWork.MemberSiteSubscriptionRepository.Update(memberSubscription);
        }

        _unitOfWork.MemberSiteSubscriptionRecordRepository.Add(new MemberSiteSubscriptionRecord
        {
            CreatedUtc = utcNow,
            PaymentId = payment.Id,
            SiteSubscriptionId = siteSubscriptionPrice.SiteSubscriptionId,
            SiteSubscriptionPriceId = siteSubscriptionPrice.Id
        });

        await _unitOfWork.SaveChangesAsync();

        return PaymentWebhookProcessingResult.Successful(
            member, chapter: null, payment, siteSubscriptionPrice.Currency);
    }
}