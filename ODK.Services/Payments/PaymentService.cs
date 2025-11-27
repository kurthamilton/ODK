using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Data.Core;
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
        IPaymentProviderFactory paymentProviderFactory,
        ILoggingService loggingService,
        IMemberEmailService memberEmailService)
    {
        _loggingService = loggingService;
        _memberEmailService = memberEmailService;
        _paymentProviderFactory = paymentProviderFactory;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> ActivateSubscriptionPlan(IPaymentSettings settings, string externalId)
    {
        var provider = _paymentProviderFactory.GetPaymentProvider(settings);
        return await provider.ActivateSubscriptionPlan(externalId);
    }

    public async Task<ServiceResult> CancelSubscription(IPaymentSettings settings, string externalId)
    {
        var provider = _paymentProviderFactory.GetPaymentProvider(settings);
        var result = await provider.CancelSubscription(externalId);
        return result
            ? ServiceResult.Successful()
            : ServiceResult.Failure("Error canceling subscription");
    }    

    public async Task<string?> CreateProduct(IPaymentSettings settings, string name)
    {
        var provider = _paymentProviderFactory.GetPaymentProvider(settings);
        return await provider.CreateProduct(name);
    }

    public async Task<string?> CreateSubscriptionPlan(IPaymentSettings settings, ExternalSubscriptionPlan subscriptionPlan)
    {
        var provider = _paymentProviderFactory.GetPaymentProvider(settings);
        return await provider.CreateSubscriptionPlan(subscriptionPlan);
    }

    public async Task<ServiceResult> DeactivateSubscriptionPlan(IPaymentSettings settings, string externalId)
    {
        var provider = _paymentProviderFactory.GetPaymentProvider(settings);
        return await provider.DeactivateSubscriptionPlan(externalId);
    }

    public async Task<ExternalCheckoutSession?> GetCheckoutSession(IPaymentSettings settings, string externalId)
    {
        var provider = _paymentProviderFactory.GetPaymentProvider(settings);
        return await provider.GetCheckoutSession(externalId);
    }

    public async Task<string?> GetProductId(IPaymentSettings settings, string name)
    {
        var provider = _paymentProviderFactory.GetPaymentProvider(settings);
        return await provider.GetProductId(name);
    }

    public async Task<ExternalSubscription?> GetSubscription(IPaymentSettings settings, string externalId)
    {
        var provider = _paymentProviderFactory.GetPaymentProvider(settings);
        return await provider.GetSubscription(externalId);
    }

    public async Task<ExternalSubscriptionPlan?> GetSubscriptionPlan(IPaymentSettings settings, string externalId)
    {
        var provider = _paymentProviderFactory.GetPaymentProvider(settings);
        return await provider.GetSubscriptionPlan(externalId);
    }

    public async Task<ServiceResult> MakePayment(
        ChapterPaymentSettings chapterPaymentSettings, 
        Currency currency, 
        Member member, 
        decimal amount, 
        string cardToken, 
        string reference)
    {
        var sitePaymentSettings = await _unitOfWork.SitePaymentSettingsRepository.GetActive().Run();

        var sitePaymentProvider = _paymentProviderFactory.GetPaymentProvider(sitePaymentSettings);

        var paymentProvider = chapterPaymentSettings.HasApiKey
            ? _paymentProviderFactory.GetPaymentProvider(chapterPaymentSettings)
            : sitePaymentProvider;

        var paymentResult = await paymentProvider.MakePayment(
            currency.Code, 
            amount, 
            cardToken, 
            reference,
            member.Id,
            member.FullName);
        if (!paymentResult.Success)
        {
            await _loggingService.Error(
                $"Error making payment for member {member.Id} for {amount} with reference '{reference}': {paymentResult.Message}");
            return ServiceResult.Failure(paymentResult.Message);
        }

        var payment = new Payment
        {
            Amount = amount,
            ChapterId = chapterPaymentSettings.ChapterId,
            CurrencyId = currency.Id,
            ExternalId = paymentResult.Id,
            MemberId = member.Id,
            PaidUtc = DateTime.UtcNow,
            Reference = reference
        };
        _unitOfWork.PaymentRepository.Add(payment);
        await _unitOfWork.SaveChangesAsync();

        if (!chapterPaymentSettings.HasApiKey && !string.IsNullOrEmpty(chapterPaymentSettings.EmailAddress))
        {
            amount = Math.Round(amount - (0.025M * amount), 2);
            await sitePaymentProvider.SendPayment(currency.Code, amount,
                chapterPaymentSettings.EmailAddress, payment.Id.ToString(), reference);
        }        

        return ServiceResult.Successful();
    }

    public async Task ProcessWebhook(
        ServiceRequest request, PlatformType platform, PaymentProviderWebhook webhook)
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

        if (string.IsNullOrEmpty(webhook.PaymentId))
        {
            var message =
                $"Received {webhook.PaymentProviderType} webhook '{webhook.Id}' for event without PaymentId; not processing";

            await _loggingService.Error(message);

            await _unitOfWork.SaveChangesAsync();
            return;
        }

        if (!webhook.Complete)
        {
            var message =
                $"Received {webhook.PaymentProviderType} webhook '{webhook.Id}' for incomplete event; not processing";

            await _loggingService.Warn(message);

            await _unitOfWork.SaveChangesAsync();
            return;
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

            await _unitOfWork.SaveChangesAsync();
            return;
        }        

        // Load basic metadata objects
        var (member, payment, paymentCheckoutSession) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(metadata.MemberId.Value),
            x => x.PaymentRepository.GetById(metadata.PaymentId.Value),
            x => x.PaymentCheckoutSessionRepository.GetById(metadata.PaymentCheckoutSessionId.Value));

        var utcNow = DateTime.UtcNow;

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

        // Update chapter subscription
        Chapter? chapter = null;
        ChapterPaymentSettings? chapterPaymentSettings = null;
        ChapterSubscription? chapterSubscription = null;

        if (metadata.ChapterId != null && metadata.ChapterSubscriptionId != null)
        {
            (chapter, chapterSubscription, chapterPaymentSettings) = await _unitOfWork.RunAsync(
                x => x.ChapterRepository.GetById(metadata.ChapterId.Value),
                x => x.ChapterSubscriptionRepository.GetById(metadata.ChapterSubscriptionId.Value),
                x => x.ChapterPaymentSettingsRepository.GetByChapterId(metadata.ChapterId.Value));

            var memberChapter = member.MemberChapter(chapterSubscription.ChapterId);
            OdkAssertions.Exists(memberChapter);

            if (chapter.Id != chapterSubscription.ChapterId)
            {
                var message = 
                    $"Not updating member chapter subscription in {webhook.PaymentProviderType} webhook: " +
                    $"chapter {chapter.Id} does not match chapter subscription {chapterSubscription.Id} chapter - {chapterSubscription.ChapterId}";
                await _loggingService.Warn(message);
            }
            else
            {
                // Recurring subscriptions use SubscriptionId, one-off payments use PaymentId
                var externalId = !string.IsNullOrEmpty(webhook.SubscriptionId)
                    ? webhook.SubscriptionId
                    : webhook.PaymentId;

                var (memberSubscription, existingMemberSubscriptionRecord) = await _unitOfWork.RunAsync(
                    x => x.MemberSubscriptionRepository.GetByMemberId(member.Id, metadata.ChapterId.Value),
                    x => x.MemberSubscriptionRecordRepository.GetByExternalIdOrDefault(externalId));

                if (existingMemberSubscriptionRecord == null)
                {
                    memberSubscription ??= new();

                    var expiresUtc = memberSubscription.ExpiresUtc > DateTime.UtcNow ? memberSubscription.ExpiresUtc.Value : DateTime.UtcNow;
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

                    var memberSubscriptionRecord = new MemberSubscriptionRecord
                    {
                        Amount = chapterSubscription.Amount,
                        ChapterId = chapterSubscription.ChapterId,
                        ChapterSubscriptionId = chapterSubscription.Id,
                        ExternalId = externalId,
                        MemberId = member.Id,
                        Months = chapterSubscription.Months,
                        PaymentId = payment.Id,
                        PurchasedUtc = utcNow,
                        Type = chapterSubscription.Type
                    };
                    _unitOfWork.MemberSubscriptionRecordRepository.Add(memberSubscriptionRecord);                    
                }
            }                     
        }

        await _unitOfWork.SaveChangesAsync();

        // send chapter payment notification
        if (chapterPaymentSettings?.UseSitePaymentProvider == true)
        {
            var siteEmailSettings = await _unitOfWork.SiteEmailSettingsRepository.Get(platform).Run();
            var currency = chapterPaymentSettings.Currency;

            await _memberEmailService.SendPaymentNotification(request, payment, currency, siteEmailSettings);
        }
    }

    public async Task<ExternalCheckoutSession> StartCheckoutSession(
        ServiceRequest request,
        IPaymentSettings settings, 
        ExternalSubscriptionPlan subscriptionPlan,
        string returnPath,
        PaymentMetadataModel metadata)
    {
        var provider = _paymentProviderFactory.GetPaymentProvider(settings);
        return await provider.StartCheckout(request, subscriptionPlan, returnPath, metadata);
    }

    public async Task UpdatePaymentMetadata(IPaymentSettings settings, string externalId, PaymentMetadataModel metadata)
    {
        var provider = _paymentProviderFactory.GetPaymentProvider(settings);
        await provider.UpdatePaymentMetadata(externalId, metadata);
    }
}
