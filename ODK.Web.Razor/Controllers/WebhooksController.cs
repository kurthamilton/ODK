using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Platforms;
using ODK.Core.Utils;
using ODK.Services.Emails;
using ODK.Services.Exceptions;
using ODK.Services.Logging;
using ODK.Services.Payments;
using ODK.Services.Tasks;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Common.Settings;

namespace ODK.Web.Razor.Controllers;

[ApiController]
[IgnoreAntiforgeryToken] // external POSTs; authenticated by signature/secret, not a token
public class WebhooksController : OdkControllerBase
{
    private readonly WebhooksControllerSettings _settings;
    private readonly IEmailService _emailService;
    private readonly ILoggingService _loggingService;
    private readonly IPaymentService _paymentService;
    private readonly IStripeWebhookParser _stripeWebhookParser;

    public WebhooksController(
        ILoggingService loggingService,
        IPaymentService paymentService,
        IStripeWebhookParser stripeWebhookParser,
        IRequestStore requestStore,
        IEmailService emailService,
        WebhooksControllerSettings settings,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
        _settings = settings;
        _emailService = emailService;
        _loggingService = loggingService;
        _paymentService = paymentService;
        _stripeWebhookParser = stripeWebhookParser;
    }

    [HttpPost("webhooks/brevo")]
    public async Task Brevo()
    {
        var env = GetHeader(_settings.BrevoWebhookEnvHeader);
        if (env != _settings.BrevoWebhookEnv)
        {
            return;
        }

        var password = GetHeader(_settings.BrevoWebhookPasswordHeader);
        if (password != _settings.BrevoWebhookPassword)
        {
            throw new OdkNotAuthenticatedException();
        }

        var json = await ReadBodyText();

        var node = JsonNode.Parse(json);
        if (node == null)
        {
            return;
        }

        var eventName = node?["event"]?.GetValue<string>();
        var externalId = node?["message-id"]?.GetValue<string>();

        if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(externalId))
        {
            await _loggingService.Error(
                $"Error processing Brevo webhook: event {eventName} or messageId {externalId}  not found");
            return;
        }

        await _emailService.AddEvent(externalId, eventName);
    }

    /// <summary>
    /// v = 1 for site webhooks.
    /// v = 2 for connected account webhooks.
    /// p = the platform whose Stripe account the endpoint belongs to, as registered in its dashboard.
    /// </summary>
    [HttpPost("webhooks/stripe")]
    public async Task Stripe(int v, PlatformType? p)
    {
        var signature = Request.Headers["Stripe-Signature"];
        var json = await ReadBodyText();

        /* The platform arrives on the URL because it cannot be read from the payload: validating the payload
           needs the signing secret, and each platform's Stripe account has its own, so the platform has to be
           known first. It grants nothing - it only chooses which secret to verify against, and a call naming
           the wrong one fails that check. An endpoint registered before the platform was on the URL is Drunken
           Knitwits', which is also where an unusable value lands, so it fails the check rather than the
           lookup. */
        var platform = p is null or PlatformType.None ? PlatformType.DrunkenKnitwits : p.Value;

        var webhook = await _stripeWebhookParser.ParseWebhook(platform, json, signature, v);
        if (webhook == null)
        {
            return;
        }

        // Only log our parsed data to avoid logging any PII in the raw JSON
        await _loggingService.Info($"Received Stripe webhook: {JsonUtils.Serialize(webhook)}");

        _paymentService.EnqueueProcessWebhookJob(JobRequest.Create(ServiceRequest), webhook);
    }
}