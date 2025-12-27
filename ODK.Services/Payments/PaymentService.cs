using ODK.Core;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Subscriptions;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.Payments.Models;

namespace ODK.Services.Payments;

public class PaymentService : IPaymentService
{
    private readonly ILoggingService _loggingService;
    private readonly IMemberEmailService _memberEmailService;
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(
        IUnitOfWork unitOfWork, 
        ILoggingService loggingService,
        IMemberEmailService memberEmailService,
        IPaymentProviderFactory paymentProviderFactory)
    {
        _loggingService = loggingService;
        _memberEmailService = memberEmailService;
        _paymentProviderFactory = paymentProviderFactory;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaymentStatusType> GetMemberChapterPaymentCheckoutSessionStatus(
        MemberChapterServiceRequest request, string externalSessionId)
    {
        var (chapterPaymentSettings, sitePaymentSettings, paymentAccount, checkoutSession) = await _unitOfWork.RunAsync(
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(request.ChapterId),
            x => x.SitePaymentSettingsRepository.GetAll(),
            x => x.ChapterPaymentAccountRepository.GetByChapterId(request.ChapterId),
            x => x.PaymentCheckoutSessionRepository.GetByMemberId(request.CurrentMemberId, externalSessionId));

        OdkAssertions.Exists(checkoutSession);

        if (checkoutSession.CompletedUtc != null)
        {
            return PaymentStatusType.Complete;
        }

        var payment = await _unitOfWork.PaymentRepository.GetById(checkoutSession.PaymentId).Run();

        var paymentProvider = _paymentProviderFactory.GetPaymentProvider(
            chapterPaymentSettings, 
            sitePaymentSettings,
            paymentAccount);

        var externalSession = await paymentProvider.GetCheckoutSession(externalSessionId);

        return externalSession == null || externalSession.Metadata.ChapterId != request.ChapterId
            ? PaymentStatusType.Expired
            : PaymentStatusType.Pending;
    }

    public async Task<PaymentStatusType> GetMemberSitePaymentCheckoutSessionStatus(
        MemberServiceRequest request, string externalSessionId)
    {
        var (checkoutSession, sitePaymentSettings) = await _unitOfWork.RunAsync(
            x => x.PaymentCheckoutSessionRepository.GetByMemberId(request.CurrentMemberId, externalSessionId),
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

        return externalSession == null
            ? PaymentStatusType.Expired
            : PaymentStatusType.Pending;
    }

    public async Task ProcessWebhook(
        ServiceRequest request, PaymentProviderWebhook webhook)
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

        if (webhook.Type == PaymentProviderWebhookType.CheckoutSessionCompleted)
        {
            result = await ProcessWebhookPayment(webhook);
        }
        else if (webhook.Type == PaymentProviderWebhookType.InvoicePaymentSucceeded)
        {
            result = await ProcessWebhookSubscription(request, webhook);
        }
        else if (webhook.Type == PaymentProviderWebhookType.PaymentSucceeded)
        {
            result = await ProcessWebhookPayment(webhook);
        }
        else
        {
            result = PaymentWebhookProcessingResult.Failure();
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
            var siteEmailSettings = await _unitOfWork.SiteEmailSettingsRepository
                .Get(request.Platform).Run();
            var (member, currency, payment) = (result.Member, result.Currency, result.Payment);

            await _memberEmailService.SendPaymentNotification(request, member, payment, currency, siteEmailSettings);
        }
    }    

    private async Task<PaymentWebhookProcessingResult> ProcessWebhookPayment(PaymentProviderWebhook webhook)
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

            await _loggingService.Warn(message);;
            return PaymentWebhookProcessingResult.Failure();
        }

        // Validate basic metadata
        var metadata = PaymentMetadataModel.FromDictionary(webhook.Metadata);

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
                $"Cannot process {webhook.PaymentProviderType} webhook '{webhook.Id}': " +
                $"metadata missing properties {string.Join(", ", missingProperties)}";

            await _loggingService.Error(message);
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
                $"Not updating Payment {payment.Id} in {webhook.PaymentProviderType} webhook processing: " +
                $"already paid";
            await _loggingService.Warn(message);
        }
        else
        {
            payment.ExternalId = webhook.PaymentId;
            payment.PaidUtc = utcNow;
            _unitOfWork.PaymentRepository.Update(payment);
        }

        // update payment checkout session
        if (paymentCheckoutSession.CompletedUtc != null)
        {
            var message =
                $"Not updating PaymentCheckoutSession {paymentCheckoutSession.Id} in {webhook.PaymentProviderType} webhook processing: " +
                $"already completed";
            await _loggingService.Warn(message);
        }
        else
        {
            paymentCheckoutSession.CompletedUtc = utcNow;
            _unitOfWork.PaymentCheckoutSessionRepository.Update(paymentCheckoutSession);
        }

        return await UpdateMemberChapterSubscription(
            metadata,
            member,
            payment,
            externalId: webhook.PaymentId,
            utcNow);        
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
                $"Cannot process {webhook.PaymentProviderType} webhook '{webhook.Id}': " +
                $"metadata missing properties {string.Join(", ", missingProperties)}";

            await _loggingService.Error(message);
            return PaymentWebhookProcessingResult.Failure();
        }

        // Load basic metadata objects
        var (member, chapter, chapterPaymentSettings, chapterSubscription, payment, paymentCheckoutSession) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(metadata.MemberId.Value),
            x => x.ChapterSubscriptionRepository.GetByIdOrDefault(metadata.ChapterSubscriptionId.Value),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(metadata.ChapterId.Value),
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
                Amount = webhook.Amount,
                ChapterId = chapter.Id,
                CreatedUtc = utcNow,
                CurrencyId = chapterPaymentSettings.CurrencyId,
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
                $"Not updating Payment {payment.Id} in {webhook.PaymentProviderType} webhook processing: " +
                $"already paid";
            await _loggingService.Warn(message);
        }
        else
        {
            payment.ExternalId = webhook.SubscriptionId;
            payment.PaidUtc = utcNow;
            _unitOfWork.PaymentRepository.Upsert(payment);
        }

        // update payment checkout session
        if (paymentCheckoutSession != null)
        {
            if (paymentCheckoutSession.CompletedUtc != null)
            {
                var message =
                    $"Not updating PaymentCheckoutSession {paymentCheckoutSession.Id} in {webhook.PaymentProviderType} webhook processing: " +
                    $"already completed";
                await _loggingService.Warn(message);
            }
            else
            {
                paymentCheckoutSession.CompletedUtc = utcNow;
                _unitOfWork.PaymentCheckoutSessionRepository.Update(paymentCheckoutSession);
            }
        }

        return await UpdateMemberChapterSubscription(
            metadata,
            member,
            payment,
            externalId: webhook.SubscriptionId,
            utcNow);
    }

    private async Task<PaymentWebhookProcessingResult> ProcessWebhookSiteSubscription(
        ServiceRequest request,
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

        if (metadata.MemberId == null ||
            metadata.SiteSubscriptionPriceId == null)
        {
            var missingProperties = new[]
            {
                metadata.MemberId == null ? "MemberId" : null,
                metadata.SiteSubscriptionPriceId == null ? "SiteSubscriptionPriceId" : null
            }.Where(x => x != null);

            var message =
                $"Cannot process {webhook.PaymentProviderType} webhook '{webhook.Id}': " +
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
                Amount = webhook.Amount,
                CreatedUtc = utcNow,
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
                $"Not updating Payment {payment.Id} in {webhook.PaymentProviderType} webhook processing: " +
                $"already paid";
            await _loggingService.Warn(message);
        }
        else
        {
            payment.ExternalId = webhook.SubscriptionId;
            payment.PaidUtc = utcNow;
            _unitOfWork.PaymentRepository.Upsert(payment);
        }

        // update payment checkout session
        if (paymentCheckoutSession != null)
        {
            if (paymentCheckoutSession.CompletedUtc != null)
            {
                var message =
                    $"Not updating PaymentCheckoutSession {paymentCheckoutSession.Id} in {webhook.PaymentProviderType} webhook processing: " +
                    $"already completed";
                await _loggingService.Warn(message);
            }
            else
            {
                paymentCheckoutSession.CompletedUtc = utcNow;
                _unitOfWork.PaymentCheckoutSessionRepository.Update(paymentCheckoutSession);
            }
        }

        try
        {
            return await UpdateMemberSiteSubscription(
                request,
                member,
                siteSubscription,
                siteSubscriptionPrice,
                payment,
                externalId: webhook.SubscriptionId,
                utcNow);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private async Task<PaymentWebhookProcessingResult> ProcessWebhookSubscription(
        ServiceRequest request, PaymentProviderWebhook webhook)
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
            return await ProcessWebhookSiteSubscription(request, webhook, metadata);
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

        var (chapter, chapterSubscription, chapterPaymentSettings) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(metadata.ChapterId.Value),
            x => x.ChapterSubscriptionRepository.GetById(metadata.ChapterSubscriptionId.Value),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(metadata.ChapterId.Value));

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
            member, payment, chapterPaymentSettings.Currency);
    }

    private async Task<PaymentWebhookProcessingResult> UpdateMemberSiteSubscription(
        ServiceRequest request,
        Member member,
        SiteSubscription siteSubscription,
        SiteSubscriptionPrice siteSubscriptionPrice,
        Payment payment,
        string externalId,
        DateTime utcNow)
    {        
        var memberId = member.Id;

        var memberSubscription = await _unitOfWork.MemberSiteSubscriptionRepository
            .GetByMemberId(memberId, request.Platform).Run();

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

        await _unitOfWork.SaveChangesAsync();

        return PaymentWebhookProcessingResult.Successful(
            member, payment, siteSubscriptionPrice.Currency);
    }
}
