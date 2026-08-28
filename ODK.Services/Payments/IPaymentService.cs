using ODK.Core.Payments;
using ODK.Services.Payments.Models;
using ODK.Services.Tasks;

namespace ODK.Services.Payments;

public interface IPaymentService
{
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
}