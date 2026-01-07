using ODK.Core.Payments;
using ODK.Services.Payments.Models;

namespace ODK.Services.Payments;

public interface IPaymentService
{
    Task<PaymentStatusType> GetMemberChapterPaymentCheckoutSessionStatus(
        MemberChapterServiceRequest request, string externalSessionId);

    Task<PaymentStatusType> GetMemberSitePaymentCheckoutSessionStatus(
        MemberServiceRequest request, string externalSessionId);

    Task ProcessWebhook(ServiceRequest request, PaymentProviderWebhook webhook);
}
