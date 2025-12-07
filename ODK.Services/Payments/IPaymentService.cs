using ODK.Services.Payments.Models;

namespace ODK.Services.Payments;

public interface IPaymentService
{
    Task ProcessWebhook(ServiceRequest request, PaymentProviderWebhook webhook);    
}
