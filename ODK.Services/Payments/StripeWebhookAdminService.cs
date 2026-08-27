using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Data.Core;
using ODK.Services.Logging;
using ODK.Services.Payments.Models;
using ODK.Services.Payments.ViewModels;

namespace ODK.Services.Payments;

public class StripeWebhookAdminService : OdkAdminServiceBase, IStripeWebhookAdminService
{
    private const string AccountPlaceholder = "{account}";

    private const string WebhookPlaceholder = "{id}";

    private readonly ILoggingService _loggingService;
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly StripeWebhookAdminServiceSettings _settings;

    public StripeWebhookAdminService(
        IUnitOfWork unitOfWork,
        IPaymentProviderFactory paymentProviderFactory,
        ILoggingService loggingService,
        StripeWebhookAdminServiceSettings settings)
        : base(unitOfWork)
    {
        _loggingService = loggingService;
        _paymentProviderFactory = paymentProviderFactory;
        _settings = settings;
    }

    public async Task<SiteAdminStripeWebhooksViewModel> GetStripeWebhooksViewModel(IMemberServiceRequest request)
    {
        var paymentSettings = await GetSiteAdminRestrictedContent(request,
            x => x.SitePaymentSettingsRepository.GetAll(request.Platform));

        var stripeSettings = paymentSettings
            .Where(x => x.Provider == PaymentProviderType.Stripe)
            // A record stating no environment sorts first: it is the one finding here fixed in the app itself.
            .OrderBy(x => x.Environment)
            .ThenBy(x => x.Name)
            .ToArray();

        var viewModels = new List<SiteAdminStripeWebhookAccountViewModel>();

        /* Sequential rather than awaited together: each call goes out to Stripe, but the failure path logs,
           and every repository and the logging service share one DbContext. A fan-out would be a
           thread-safety bug the moment one of these branches touched either. */
        foreach (var paymentSetting in stripeSettings)
        {
            viewModels.Add(await GetAccountViewModel(paymentSetting));
        }

        return new SiteAdminStripeWebhooksViewModel
        {
            Accounts = viewModels
        };
    }

    private static SiteAdminStripeWebhookAccountViewModel Unreadable(SitePaymentSettings paymentSettings, string error)
        => new()
        {
            /* Nothing is reported as missing or duplicated, because nothing was read - an account that could
               not be listed is not an account with no endpoints. */
            DisabledWebhooks = [],
            DuplicateKinds = [],
            EnvironmentNotSet = paymentSettings.Environment is null or EnvironmentType.None,
            Error = error,
            MissingKinds = [],
            MixedApiVersions = false,
            PaymentSettings = paymentSettings,
            Webhooks = []
        };

    private string? DashboardUrl(SitePaymentSettings paymentSettings, StripeWebhookEndpoint endpoint)
    {
        var format = endpoint.LiveMode
            ? _settings.LiveDashboardUrlFormat
            : _settings.TestDashboardUrlFormat;

        return !string.IsNullOrWhiteSpace(paymentSettings.ExternalId) && !string.IsNullOrWhiteSpace(format)
            ? format
                .Replace(AccountPlaceholder, paymentSettings.ExternalId)
                .Replace(WebhookPlaceholder, endpoint.Id)
            : null;
    }

    private async Task<SiteAdminStripeWebhookAccountViewModel> GetAccountViewModel(SitePaymentSettings paymentSettings)
    {
        var provider = _paymentProviderFactory.GetStripeWebhookProvider(paymentSettings);
        if (provider == null)
        {
            return Unreadable(paymentSettings, "Provider does not support webhooks");
        }

        IReadOnlyCollection<StripeWebhookEndpoint> endpoints;

        try
        {
            endpoints = await provider.ListWebhooks();
        }
        catch (Exception ex)
        {
            await _loggingService.Error($"Error listing Stripe webhooks for '{paymentSettings.Name}'", ex);
            return Unreadable(paymentSettings, ex.Message);
        }

        var audit = StripeWebhookAudit.Audit(paymentSettings, endpoints, _settings);

        return new SiteAdminStripeWebhookAccountViewModel
        {
            DisabledWebhooks =
            [
                .. audit.DisabledEndpoints.Select(x => ToViewModel(paymentSettings, x))
            ],
            DuplicateKinds = audit.DuplicateKinds,
            EnvironmentNotSet = audit.EnvironmentNotSet,
            Error = null,
            MissingKinds = audit.MissingKinds,
            MixedApiVersions = audit.MixedApiVersions,
            PaymentSettings = paymentSettings,
            Webhooks = [.. audit.Endpoints.Select(x => ToViewModel(paymentSettings, x))]
        };
    }

    private SiteAdminStripeWebhookViewModel ToViewModel(
        SitePaymentSettings paymentSettings,
        StripeWebhookEndpointAudit audit)
        => new()
        {
            ApiVersion = audit.Endpoint.ApiVersion,
            Checks = audit.Checks,
            DashboardUrl = DashboardUrl(paymentSettings, audit.Endpoint),
            Events = audit.Endpoint.Events,
            ExtraEvents = audit.ExtraEvents,
            Id = audit.Endpoint.Id,
            Kind = audit.Kind,
            MissingEvents = audit.MissingEvents,
            Url = audit.Endpoint.Url
        };
}
