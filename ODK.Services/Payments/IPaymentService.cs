using ODK.Core.Payments;
using ODK.Services.Payments.Models;
using ODK.Services.Tasks;

namespace ODK.Services.Payments;

public interface IPaymentService
{
    /// <summary>
    /// Queues <see cref="EnsureProductExists"/>. The group is named by
    /// <see cref="JobRequest.ChapterId"/>, so the job fails rather than runs if it has been deleted by the
    /// time it is picked up.
    /// </summary>
    string EnqueueEnsureProductExistsJob(JobRequest request);

    /// <summary>
    /// Queues <see cref="ProcessWebhook"/>.
    /// </summary>
    string EnqueueProcessWebhookJob(JobRequest request, PaymentProviderWebhook webhook);

    Task EnsureProductExists(IChapterServiceRequest request);

    Task<PaymentStatusType> GetMemberChapterPaymentCheckoutSessionStatus(
        IMemberServiceRequest request, Guid chapterId, string externalSessionId);

    Task<PaymentStatusType> GetMemberSitePaymentCheckoutSessionStatus(
        IMemberServiceRequest request, string externalSessionId);

    Task ProcessWebhook(IServiceRequest request, PaymentProviderWebhook webhook);
}