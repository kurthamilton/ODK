using ODK.Core.Chapters;
using ODK.Core.Payments;
using ODK.Core.Subscriptions;
using ODK.Services.Payments.Models;
using ODK.Services.Tasks;

namespace ODK.Services.Payments;

public interface IPaymentService
{
    Task<(Payment Payment, ExternalCheckoutSession Session, string PublicApiKey)> CreateChapterOneOffPayment(
        IMemberChapterServiceRequest request,
        ChapterPaymentAccount paymentAccount,
        OneOffPaymentCreateOptions options);

    Task<(Payment Payment, ExternalCheckoutSession Session, string PublicApiKey)> CreateChapterPayment(
        IMemberChapterServiceRequest request,
        ChapterPaymentAccount paymentAccount,
        ChapterSubscription subscription,
        PaymentCreateOptions options);

    Task<(Payment Payment, ExternalCheckoutSession Session, string PublicApiKey)> CreateSitePayment(
        IMemberServiceRequest request,
        SiteSubscription subscription,
        SiteSubscriptionPrice price,
        PaymentCreateOptions options);

    /// <summary>
    /// Queues the creation of the payment provider's product for a group, where it has none. The group is
    /// named by <see cref="JobRequest.ChapterId"/>, so the job fails rather than runs if it has been deleted
    /// by the time it is picked up.
    /// </summary>
    string EnqueueEnsureProductExistsJob(JobRequest request);

    /// <summary>
    /// Queues the recording of a received webhook, which in turn queues the actioning of it.
    /// </summary>
    string EnqueueProcessWebhookJob(JobRequest request, PaymentProviderWebhook webhook);

    /// <summary>
    /// Queues the reading of what a payment settled at, for a payment whose settlement is not already
    /// recorded. The ids the webhook would have supplied are not known here, so the job falls back to the
    /// reference recorded when the payment was taken.
    /// </summary>
    string EnqueueResolvePaymentSettlementJob(Guid paymentId);

    Task<PaymentStatusType> GetMemberChapterPaymentCheckoutSessionStatus(
        IMemberServiceRequest request, Guid chapterId, string externalSessionId);

    Task<PaymentStatusType> GetMemberSitePaymentCheckoutSessionStatus(
        IMemberServiceRequest request, string externalSessionId);

    /// <summary>
    /// Reconciles one payment now and reports what happened, for a caller waiting on the answer rather than
    /// queueing the work. Unlike the queued job, a state that would earn a retry comes back as a failure
    /// instead of being thrown - so this is for acting on a single payment where something is waiting to be
    /// told, not for a sweep.
    /// </summary>
    Task<ResolvePaymentSettlementResult> ResolvePaymentSettlement(Guid paymentId);
}