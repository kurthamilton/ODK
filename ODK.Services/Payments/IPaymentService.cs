using ODK.Core.Payments;
using ODK.Services.Payments.Models;

namespace ODK.Services.Payments;

public interface IPaymentService
{
    Task EnsureProductExists(IChapterServiceRequest request);

    Task<PaymentStatusType> GetMemberChapterPaymentCheckoutSessionStatus(
        IMemberServiceRequest request, Guid chapterId, string externalSessionId);

    Task<PaymentStatusType> GetMemberSitePaymentCheckoutSessionStatus(
        IMemberServiceRequest request, string externalSessionId);

    Task ProcessWebhook(IServiceRequest request, PaymentProviderWebhook webhook);
}