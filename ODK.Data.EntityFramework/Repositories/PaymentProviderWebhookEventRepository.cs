using Microsoft.EntityFrameworkCore;
using ODK.Core.Payments;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class PaymentProviderWebhookEventRepository : WriteRepositoryBase<PaymentProviderWebhookEvent>, IPaymentProviderWebhookEventRepository
{
    public PaymentProviderWebhookEventRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<PaymentProviderWebhookEvent> GetByExternalId(
        PaymentProviderType paymentProviderType, string externalId)
        => Set()
            .Where(x => x.PaymentProviderType == paymentProviderType && x.ExternalId == externalId)
            .DeferredSingleOrDefault();
}