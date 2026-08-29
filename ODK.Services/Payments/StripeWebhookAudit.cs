using ODK.Core.Platforms;
using ODK.Services.Payments.Models;

namespace ODK.Services.Payments;

/// <summary>
/// Compares the webhook endpoints a Stripe account actually has against what one is expected to have.
/// </summary>
/// <remarks>
/// Pure and dependency-free, so every rule is reachable from a test without a Stripe account. Whatever
/// cannot be compared is reported as such - see <see cref="StripeWebhookCheckState"/>.
/// </remarks>
public static class StripeWebhookAudit
{
    private const string PlatformParameter = "p";

    private const string VersionParameter = "v";

    /// <summary>Every event, which Stripe accepts in place of naming them.</summary>
    private const string WildcardEvent = "*";

    public static StripeWebhookAuditResult Audit(
        StripePaymentAccount account,
        IReadOnlyCollection<StripeWebhookEndpoint> endpoints,
        StripeWebhookAdminServiceSettings expectations)
    {
        var environment = account.Environment;

        /* A disabled endpoint is not a participant: Stripe will not deliver to it, so it is superseded
           rather than broken, and judging it would report a finding against a decision somebody made on
           purpose. It counts towards nothing below - which is what makes disabling the only endpoint for a
           kind show up as that kind having none, rather than as one that exists and does nothing. */
        var audited = endpoints
            .Where(x => x.Enabled)
            .Select(x => AuditEndpoint(account, environment, x, expectations))
            .OrderBy(x => x.Kind == StripeWebhookKind.None)
            .ThenBy(x => x.Kind)
            .ThenBy(x => x.Endpoint.Url)
            .ToArray();

        var kinds = Enum
            .GetValues<StripeWebhookKind>()
            .Where(x => x != StripeWebhookKind.None)
            .ToArray();

        var countsByKind = kinds.ToDictionary(kind => kind, kind => audited.Count(x => x.Kind == kind));

        return new StripeWebhookAuditResult
        {
            DisabledEndpoints =
            [
                .. endpoints
                    .Where(x => !x.Enabled)
                    .OrderBy(x => x.Url)
                    .Select(Unjudged)
            ],
            DuplicateKinds = [.. kinds.Where(x => countsByKind[x] > 1)],
            Endpoints = audited,
            EnvironmentNotSet = environment == EnvironmentType.None,
            MissingKinds = [.. kinds.Where(x => countsByKind[x] == 0)],
            /* Nulls count: an endpoint with no pinned version renders events as the account default, so one
               pinned beside one unpinned is the same drift as two different pins. */
            MixedApiVersions = audited.Select(x => x.Endpoint.ApiVersion).Distinct().Count() > 1
        };
    }

    private static StripeWebhookEndpointAudit AuditEndpoint(
        StripePaymentAccount account,
        EnvironmentType environment,
        StripeWebhookEndpoint endpoint,
        StripeWebhookAdminServiceSettings expectations)
    {
        var uri = Uri.TryCreate(endpoint.Url, UriKind.Absolute, out var parsed) ? parsed : null;
        var query = ParseQuery(uri);

        var kind = ReadKind(query);

        var missingEvents = MissingEvents(endpoint, expectations);

        StripeWebhookCheck[] checks =
        [
            EventsCheck(expectations, missingEvents),
            HostCheck(account, environment, uri, expectations),
            LiveModeCheck(environment, endpoint),
            PathCheck(uri, expectations),
            PlatformCheck(account, query),
            QueryCheck(query),
            VersionCheck(query, kind)
        ];

        return new StripeWebhookEndpointAudit
        {
            Checks = checks,
            Endpoint = endpoint,
            ExtraEvents = ExtraEvents(endpoint, expectations),
            Kind = kind,
            MissingEvents = missingEvents
        };
    }

    /// <summary>
    /// The scheme and authority of a URL, or the URL itself where it will not parse - so a comparison
    /// against a malformed expectation shows both values rather than passing on a null.
    /// </summary>
    private static string Authority(string url)
        => Uri.TryCreate(url, UriKind.Absolute, out var uri)
            ? uri.GetLeftPart(UriPartial.Authority)
            : url.TrimEnd('/');

    private static StripeWebhookCheck EventsCheck(
        StripeWebhookAdminServiceSettings expectations,
        IReadOnlyCollection<string> missingEvents)
        => new()
        {
            // The lists themselves are on the audit, where a reader can see which events are missing.
            Actual = null,
            Expected = null,
            Severity = StripeWebhookCheckSeverity.Error,
            State = expectations.Events.Count == 0
                ? StripeWebhookCheckState.NotComparable
                : missingEvents.Count == 0
                    ? StripeWebhookCheckState.Met
                    : StripeWebhookCheckState.Unmet,
            Type = StripeWebhookCheckType.Events
        };

    private static IReadOnlyCollection<string> ExtraEvents(
        StripeWebhookEndpoint endpoint,
        StripeWebhookAdminServiceSettings expectations)
        => endpoint.Events.Contains(WildcardEvent) || expectations.Events.Count == 0
            ? []
            : [.. endpoint.Events.Except(expectations.Events, StringComparer.OrdinalIgnoreCase)];

    private static StripeWebhookCheck HostCheck(
        StripePaymentAccount account,
        EnvironmentType environment,
        Uri? uri,
        StripeWebhookAdminServiceSettings expectations)
    {
        var expected = expectations.Hosts.TryGetValue(environment, out var platformHosts)
            && platformHosts.TryGetValue(account.Platform, out var host)
            && !string.IsNullOrWhiteSpace(host)
                ? host
                : null;

        var actual = uri?.GetLeftPart(UriPartial.Authority);

        return new StripeWebhookCheck
        {
            Actual = actual,
            Expected = expected,
            Severity = StripeWebhookCheckSeverity.Error,
            State = expected == null
                ? StripeWebhookCheckState.NotComparable
                : string.Equals(actual, Authority(expected), StringComparison.OrdinalIgnoreCase)
                    ? StripeWebhookCheckState.Met
                    : StripeWebhookCheckState.Unmet,
            Type = StripeWebhookCheckType.Host
        };
    }

    private static StripeWebhookCheck LiveModeCheck(EnvironmentType environment, StripeWebhookEndpoint endpoint)
    {
        // Every environment other than prod transacts through a Stripe sandbox, which is never live mode.
        var expected = environment == EnvironmentType.Prod;

        return new StripeWebhookCheck
        {
            Actual = endpoint.LiveMode.ToString(),
            Expected = environment != EnvironmentType.None ? expected.ToString() : null,
            Severity = StripeWebhookCheckSeverity.Error,
            State = environment == EnvironmentType.None
                ? StripeWebhookCheckState.NotComparable
                : endpoint.LiveMode == expected
                    ? StripeWebhookCheckState.Met
                    : StripeWebhookCheckState.Unmet,
            Type = StripeWebhookCheckType.LiveMode
        };
    }

    private static IReadOnlyCollection<string> MissingEvents(
        StripeWebhookEndpoint endpoint,
        StripeWebhookAdminServiceSettings expectations)
        => endpoint.Events.Contains(WildcardEvent)
            ? []
            : [.. expectations.Events.Except(endpoint.Events, StringComparer.OrdinalIgnoreCase)];

    /// <summary>
    /// The URL's query parameters, keyed case-insensitively. Hand-parsed rather than taken from a web
    /// framework, so the audit stays free of one and every case a test can write is a case this handles: a
    /// parameter with no value keys itself with an empty value, and a repeated one keeps the last.
    /// </summary>
    private static IReadOnlyDictionary<string, string> ParseQuery(Uri? uri)
    {
        var parsed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var query = uri?.Query.TrimStart('?') ?? string.Empty;

        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = pair.IndexOf('=');

            var key = separator >= 0 ? pair[..separator] : pair;
            var value = separator >= 0 ? pair[(separator + 1)..] : string.Empty;

            parsed[Uri.UnescapeDataString(key)] = Uri.UnescapeDataString(value);
        }

        return parsed;
    }

    private static StripeWebhookCheck PathCheck(Uri? uri, StripeWebhookAdminServiceSettings expectations)
    {
        var actual = uri?.AbsolutePath ?? string.Empty;

        return new StripeWebhookCheck
        {
            Actual = actual,
            Expected = expectations.Path,
            Severity = StripeWebhookCheckSeverity.Error,
            State = string.IsNullOrWhiteSpace(expectations.Path)
                ? StripeWebhookCheckState.NotComparable
                : string.Equals(
                    actual.TrimEnd('/'), expectations.Path.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)
                    ? StripeWebhookCheckState.Met
                    : StripeWebhookCheckState.Unmet,
            Type = StripeWebhookCheckType.Path
        };
    }

    private static StripeWebhookCheck PlatformCheck(
        StripePaymentAccount account,
        IReadOnlyDictionary<string, string> query)
    {
        query.TryGetValue(PlatformParameter, out var actual);

        /* An absent p is read by WebhooksController as Drunken Knitwits, so a Drunken Knitwits endpoint
           without one works - on a default nothing about the endpoint states. Any other platform's silently
           does not, which is why the same omission is a warning on one and an error on the other. */
        var severity = string.IsNullOrEmpty(actual) && account.Platform == PlatformType.DrunkenKnitwits
            ? StripeWebhookCheckSeverity.Warning
            : StripeWebhookCheckSeverity.Error;

        return new StripeWebhookCheck
        {
            Actual = actual,
            Expected = account.Platform.ToString(),
            Severity = severity,
            State = Enum.TryParse<PlatformType>(actual, ignoreCase: true, out var platform)
                && platform == account.Platform
                    ? StripeWebhookCheckState.Met
                    : StripeWebhookCheckState.Unmet,
            Type = StripeWebhookCheckType.Platform
        };
    }

    private static StripeWebhookCheck QueryCheck(IReadOnlyDictionary<string, string> query)
    {
        var unexpected = query
            .Keys
            .Where(x => !string.Equals(x, VersionParameter, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(x, PlatformParameter, StringComparison.OrdinalIgnoreCase))
            .Order()
            .ToArray();

        return new StripeWebhookCheck
        {
            Actual = unexpected.Length > 0 ? string.Join(", ", unexpected) : null,
            Expected = null,
            Severity = StripeWebhookCheckSeverity.Error,
            State = unexpected.Length == 0 ? StripeWebhookCheckState.Met : StripeWebhookCheckState.Unmet,
            Type = StripeWebhookCheckType.Query
        };
    }

    private static StripeWebhookKind ReadKind(IReadOnlyDictionary<string, string> query)
    {
        query.TryGetValue(VersionParameter, out var version);

        return int.TryParse(version, out var parsed) && Enum.IsDefined((StripeWebhookKind)parsed)
            ? (StripeWebhookKind)parsed
            : StripeWebhookKind.None;
    }

    /// <summary>
    /// An endpoint recorded without being compared against anything - what it is, and nothing about whether
    /// it is right. The kind is still read, so a disabled endpoint can be told apart when tidying up.
    /// </summary>
    private static StripeWebhookEndpointAudit Unjudged(StripeWebhookEndpoint endpoint)
    {
        var uri = Uri.TryCreate(endpoint.Url, UriKind.Absolute, out var parsed) ? parsed : null;

        return new StripeWebhookEndpointAudit
        {
            Checks = [],
            Endpoint = endpoint,
            ExtraEvents = [],
            Kind = ReadKind(ParseQuery(uri)),
            MissingEvents = []
        };
    }

    private static StripeWebhookCheck VersionCheck(
        IReadOnlyDictionary<string, string> query,
        StripeWebhookKind kind)
    {
        query.TryGetValue(VersionParameter, out var actual);

        return new StripeWebhookCheck
        {
            Actual = actual,
            Expected = null,
            Severity = StripeWebhookCheckSeverity.Error,
            State = kind != StripeWebhookKind.None
                ? StripeWebhookCheckState.Met
                : StripeWebhookCheckState.Unmet,
            Type = StripeWebhookCheckType.Version
        };
    }
}
