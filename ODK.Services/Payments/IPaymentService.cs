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

    Task<PaymentStatusType> GetMemberChapterPaymentCheckoutSessionStatus(
        IMemberServiceRequest request, Guid chapterId, string externalSessionId);

    Task<PaymentStatusType> GetMemberSitePaymentCheckoutSessionStatus(
        IMemberServiceRequest request, string externalSessionId);
}