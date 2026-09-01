using Microsoft.AspNetCore.SignalR;
using ODK.Services.Payments;
using ODK.Web.Razor.Hubs;

namespace ODK.Web.Razor.Services;

public class SignalRPaymentUpdateBroadcaster : IPaymentUpdateBroadcaster
{
    private readonly IHubContext<PaymentsHub> _hubContext;

    public SignalRPaymentUpdateBroadcaster(IHubContext<PaymentsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    /* Reaches only the connections this process holds. Both prod sites run their own, with no backplane
       between them, and a webhook is not guaranteed to be processed by the site the buyer is browsing - see
       ProcessWebhook on why the receiving host says nothing about the payment. So a page that hears nothing
       has to find out for itself, which is what its fallback poll is for. */
    public async Task CheckoutSessionUpdated(string externalSessionId)
        => await _hubContext.Clients
            .Group(PaymentsHub.CheckoutSessionGroup(externalSessionId))
            .SendAsync(PaymentsHub.CheckoutSessionUpdatedMessage);
}
