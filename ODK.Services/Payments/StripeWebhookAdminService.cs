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
        var (environment, platform, currentMember) =
            (request.Environment, request.Platform, request.CurrentMember);

        AssertMemberIsSiteAdmin(currentMember);

        var viewModel = await GetAccountViewModel(new StripePaymentAccount
        {
            AccountId = _settings.AccountIds[platform],
            Environment = environment,
            Platform = platform
        });

        return new SiteAdminStripeWebhooksViewModel
        {
            Accounts = [viewModel]
        };
    }

    private static SiteAdminStripeWebhookAccountViewModel Unreadable(StripePaymentAccount account, string error)
        => new()
        {
            Account = account,
            /* Nothing is reported as missing or duplicated, because nothing was read - an account that could
               not be listed is not an account with no endpoints. */
            DisabledWebhooks = [],
            DuplicateKinds = [],
            EnvironmentNotSet = account.Environment == EnvironmentType.None,
            Error = error,
            MissingKinds = [],
            MixedApiVersions = false,
            Webhooks = []
        };

    private string? DashboardUrl(StripePaymentAccount account, StripeWebhookEndpoint endpoint)
    {
        var format = endpoint.LiveMode
            ? _settings.LiveDashboardUrlFormat
            : _settings.TestDashboardUrlFormat;

        return !string.IsNullOrWhiteSpace(account.AccountId) && !string.IsNullOrWhiteSpace(format)
            ? format
                .Replace(AccountPlaceholder, account.AccountId)
                .Replace(WebhookPlaceholder, endpoint.Id)
            : null;
    }

    private async Task<SiteAdminStripeWebhookAccountViewModel> GetAccountViewModel(StripePaymentAccount account)
    {
        var provider = _paymentProviderFactory.GetStripeWebhookProvider(account.Platform);
        if (provider == null)
        {
            return Unreadable(account, "Provider does not support webhooks");
        }

        IReadOnlyCollection<StripeWebhookEndpoint> endpoints;

        try
        {
            endpoints = await provider.ListWebhooks();
        }
        catch (Exception ex)
        {
            await _loggingService.Error($"Error listing Stripe webhooks for '{account.Platform}'", ex);
            return Unreadable(account, ex.Message);
        }

        var audit = StripeWebhookAudit.Audit(account, endpoints, _settings);

        return new SiteAdminStripeWebhookAccountViewModel
        {
            Account = account,
            DisabledWebhooks =
            [
                .. audit.DisabledEndpoints.Select(x => ToViewModel(account, x))
            ],
            DuplicateKinds = audit.DuplicateKinds,
            EnvironmentNotSet = audit.EnvironmentNotSet,
            Error = null,
            MissingKinds = audit.MissingKinds,
            MixedApiVersions = audit.MixedApiVersions,
            Webhooks = [.. audit.Endpoints.Select(x => ToViewModel(account, x))]
        };
    }

    private SiteAdminStripeWebhookViewModel ToViewModel(
        StripePaymentAccount account,
        StripeWebhookEndpointAudit audit)
        => new()
        {
            ApiVersion = audit.Endpoint.ApiVersion,
            Checks = audit.Checks,
            DashboardUrl = DashboardUrl(account, audit.Endpoint),
            Events = audit.Endpoint.Events,
            ExtraEvents = audit.ExtraEvents,
            Id = audit.Endpoint.Id,
            Kind = audit.Kind,
            MissingEvents = audit.MissingEvents,
            Url = audit.Endpoint.Url
        };
}
