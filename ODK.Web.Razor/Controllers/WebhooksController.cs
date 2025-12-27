using Microsoft.AspNetCore.Mvc;
using ODK.Services.Logging;
using ODK.Services.Payments;
using ODK.Services.Tasks;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Controllers;

[ApiController]
public class WebhooksController : OdkControllerBase
{
    private readonly IBackgroundTaskService _backgroundTaskService;
    private readonly ILoggingService _loggingService;    
    private readonly IPaymentService _paymentService;
    private readonly IStripeWebhookParser _stripeWebhookParser;

    public WebhooksController(
        ILoggingService loggingService,
        IPaymentProviderFactory paymentProviderFactory,
        IBackgroundTaskService backgroundTaskService,
        IPaymentService paymentService,
        IStripeWebhookParser stripeWebhookParser,
        IRequestStore requestStore)
        : base(requestStore)
    {
        _backgroundTaskService = backgroundTaskService;
        _loggingService = loggingService;
        _paymentService = paymentService;
        _stripeWebhookParser = stripeWebhookParser;
    }

    [HttpPost("webhooks/stripe")]
    public async Task Stripe(int v)
    {
        var signature = Request.Headers["Stripe-Signature"];
        var json = await ReadBodyText();

        await _loggingService.Info($"Received Stripe webhook: {json}");

        var webhook = await _stripeWebhookParser.ParseWebhook(json, signature, v);
        if (webhook == null)
        {
            return;
        }

        _backgroundTaskService.Enqueue(
            () => _paymentService.ProcessWebhook(ServiceRequest, webhook));
    }
}
