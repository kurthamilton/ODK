using ODK.Core.Payments;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IPaymentProviderWebhookEventRepository : IWriteRepository<PaymentProviderWebhookEvent>
{
    IDeferredQuerySingleOrDefault<PaymentProviderWebhookEvent> GetByExternalId(
        PaymentProviderType paymentProviderType, string externalId);
}
