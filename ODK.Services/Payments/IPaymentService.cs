using ODK.Core.Chapters;
using ODK.Core.Payments;
using ODK.Services.Payments.Models;

namespace ODK.Services.Payments;

public interface IPaymentService
{
    Task EnsureProductExists(Guid chapterId);

    Task<PaymentStatusType> GetMemberChapterPaymentCheckoutSessionStatus(
        MemberServiceRequest request, Guid chapterId, string externalSessionId);

    Task<PaymentStatusType> GetMemberSitePaymentCheckoutSessionStatus(
        MemberServiceRequest request, string externalSessionId);

    Task ProcessWebhook(ServiceRequest request, PaymentProviderWebhook webhook);
}