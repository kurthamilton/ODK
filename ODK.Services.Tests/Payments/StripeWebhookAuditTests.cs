using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Services.Payments;
using ODK.Services.Payments.Models;

namespace ODK.Services.Tests.Payments;

[Parallelizable]
public static class StripeWebhookAuditTests
{
    private const string DefaultHost = "https://groupsquirrel.com";

    private static readonly string[] ExpectedEvents =
    [
        "checkout.session.completed",
        "checkout.session.expired"
    ];

    [Test]
    public static void Audit_WhenEndpointMatchesEverything_ReportsNothing()
    {
        // Arrange
        var paymentSettings = CreatePaymentAccount();
        var endpoints = new[] { CreateEndpoint() };

        // Act
        var result = StripeWebhookAudit.Audit(paymentSettings, endpoints, CreateExpectations());

        // Assert
        result.Endpoints
            .Single()
            .Checks
            .Where(x => x.State != StripeWebhookCheckState.Met)
            .Should()
            .BeEmpty();
    }

    [Test]
    public static void Audit_WhenAKindHasNoEndpoint_ReportsItMissing()
    {
        // Arrange
        var endpoints = new[] { CreateEndpoint(Url(StripeWebhookKind.Site)) };

        // Act
        var result = StripeWebhookAudit.Audit(CreatePaymentAccount(), endpoints, CreateExpectations());

        // Assert
        result.MissingKinds.Should().BeEquivalentTo([StripeWebhookKind.ConnectedAccount]);
    }

    [Test]
    public static void Audit_WhenBothKindsHaveAnEndpoint_ReportsNoneMissing()
    {
        // Arrange
        var endpoints = new[]
        {
            CreateEndpoint(Url(StripeWebhookKind.Site)),
            CreateEndpoint(Url(StripeWebhookKind.ConnectedAccount), id: "we_2")
        };

        // Act
        var result = StripeWebhookAudit.Audit(CreatePaymentAccount(), endpoints, CreateExpectations());

        // Assert
        result.MissingKinds.Should().BeEmpty();
    }

    [Test]
    public static void Audit_WhenTwoEndpointsClaimTheSameKind_ReportsItDuplicated()
    {
        // Arrange
        var endpoints = new[]
        {
            CreateEndpoint(Url(StripeWebhookKind.Site)),
            CreateEndpoint(Url(StripeWebhookKind.Site), id: "we_2")
        };

        // Act
        var result = StripeWebhookAudit.Audit(CreatePaymentAccount(), endpoints, CreateExpectations());

        // Assert
        result.DuplicateKinds.Should().BeEquivalentTo([StripeWebhookKind.Site]);
    }

    [Test]
    public static void Audit_WhenRecordStatesNoEnvironment_ReportsItAndComparesNeitherHostNorLiveMode()
    {
        // Arrange
        var paymentSettings = CreatePaymentAccount(environment: EnvironmentType.None);

        // Act
        var result = StripeWebhookAudit.Audit(paymentSettings, [CreateEndpoint()], CreateExpectations());

        // Assert
        result.EnvironmentNotSet.Should().BeTrue();
        Check(result, StripeWebhookCheckType.Host).State.Should().Be(StripeWebhookCheckState.NotComparable);
        Check(result, StripeWebhookCheckType.LiveMode).State.Should().Be(StripeWebhookCheckState.NotComparable);
    }

    [Test]
    public static void Audit_WhenEndpointIsMissingAnExpectedEvent_ReportsTheEventAndAnUnmetCheck()
    {
        // Arrange
        var endpoints = new[] { CreateEndpoint(events: ["checkout.session.completed"]) };

        // Act
        var result = StripeWebhookAudit.Audit(CreatePaymentAccount(), endpoints, CreateExpectations());

        // Assert
        result.Endpoints.Single().MissingEvents.Should().BeEquivalentTo(["checkout.session.expired"]);
        Check(result, StripeWebhookCheckType.Events).State.Should().Be(StripeWebhookCheckState.Unmet);
    }

    [Test]
    public static void Audit_WhenEndpointTakesEveryEvent_ReportsNoneMissing()
    {
        // Arrange
        var endpoints = new[] { CreateEndpoint(events: ["*"]) };

        // Act
        var result = StripeWebhookAudit.Audit(CreatePaymentAccount(), endpoints, CreateExpectations());

        // Assert
        result.Endpoints.Single().MissingEvents.Should().BeEmpty();
        result.Endpoints.Single().ExtraEvents.Should().BeEmpty();
        Check(result, StripeWebhookCheckType.Events).State.Should().Be(StripeWebhookCheckState.Met);
    }

    [Test]
    public static void Audit_WhenEndpointTakesAnEventNothingExpects_ReportsItWithoutFailingTheCheck()
    {
        // Arrange
        var events = ExpectedEvents.Append("customer.created").ToArray();
        var endpoints = new[] { CreateEndpoint(events: events) };

        // Act
        var result = StripeWebhookAudit.Audit(CreatePaymentAccount(), endpoints, CreateExpectations());

        // Assert
        result.Endpoints.Single().ExtraEvents.Should().BeEquivalentTo(["customer.created"]);
        Check(result, StripeWebhookCheckType.Events).State.Should().Be(StripeWebhookCheckState.Met);
    }

    [Test]
    public static void Audit_WhenNoEventsAreConfigured_ComparesNoneOfThem()
    {
        // Arrange
        var expectations = CreateExpectations(events: []);
        var endpoints = new[] { CreateEndpoint(events: ["checkout.session.completed"]) };

        // Act
        var result = StripeWebhookAudit.Audit(CreatePaymentAccount(), endpoints, expectations);

        // Assert
        Check(result, StripeWebhookCheckType.Events).State.Should().Be(StripeWebhookCheckState.NotComparable);
        result.Endpoints.Single().MissingEvents.Should().BeEmpty();
        result.Endpoints.Single().ExtraEvents.Should().BeEmpty();
    }

    [Test]
    public static void Audit_WhenEndpointAddressesAnotherPath_ReportsAnUnmetPath()
    {
        // Arrange
        var endpoints = new[] { CreateEndpoint($"{DefaultHost}/webhooks/legacy?v=1&p=Default") };

        // Act
        var result = StripeWebhookAudit.Audit(CreatePaymentAccount(), endpoints, CreateExpectations());

        // Assert
        var check = Check(result, StripeWebhookCheckType.Path);
        check.State.Should().Be(StripeWebhookCheckState.Unmet);
        check.Actual.Should().Be("/webhooks/legacy");
        check.Expected.Should().Be("/webhooks/stripe");
    }

    [Test]
    public static void Audit_WhenNoPathIsConfigured_ComparesNoPath()
    {
        // Arrange
        var expectations = CreateExpectations(path: string.Empty);

        // Act
        var result = StripeWebhookAudit.Audit(CreatePaymentAccount(), [CreateEndpoint()], expectations);

        // Assert
        Check(result, StripeWebhookCheckType.Path).State.Should().Be(StripeWebhookCheckState.NotComparable);
    }

    [Test]
    public static void Audit_WhenUrlWillNotParse_ReportsAnUnmetPathAndNoKind()
    {
        // Arrange
        var endpoints = new[] { CreateEndpoint("not a url") };

        // Act
        var result = StripeWebhookAudit.Audit(CreatePaymentAccount(), endpoints, CreateExpectations());

        // Assert
        result.Endpoints.Single().Kind.Should().Be(StripeWebhookKind.None);
        Check(result, StripeWebhookCheckType.Path).State.Should().Be(StripeWebhookCheckState.Unmet);
    }

    [TestCase("")]
    [TestCase("?p=Default")]
    [TestCase("?v=&p=Default")]
    [TestCase("?v=nine&p=Default")]
    [TestCase("?v=3&p=Default")]
    public static void Audit_WhenUrlNamesNoKnownVersion_ReportsNoKindAndAnUnmetVersion(string query)
    {
        // Arrange
        var endpoints = new[] { CreateEndpoint($"{DefaultHost}/webhooks/stripe{query}") };

        // Act
        var result = StripeWebhookAudit.Audit(CreatePaymentAccount(), endpoints, CreateExpectations());

        // Assert
        result.Endpoints.Single().Kind.Should().Be(StripeWebhookKind.None);
        Check(result, StripeWebhookCheckType.Version).State.Should().Be(StripeWebhookCheckState.Unmet);
    }

    [TestCase(1, StripeWebhookKind.Site)]
    [TestCase(2, StripeWebhookKind.ConnectedAccount)]
    public static void Audit_WhenUrlNamesAVersion_ReadsItAsThatKind(int version, StripeWebhookKind expected)
    {
        // Arrange
        var endpoints = new[] { CreateEndpoint($"{DefaultHost}/webhooks/stripe?v={version}&p=Default") };

        // Act
        var result = StripeWebhookAudit.Audit(CreatePaymentAccount(), endpoints, CreateExpectations());

        // Assert
        result.Endpoints.Single().Kind.Should().Be(expected);
        Check(result, StripeWebhookCheckType.Version).State.Should().Be(StripeWebhookCheckState.Met);
    }

    [Test]
    public static void Audit_WhenUrlNamesAnotherPlatform_ReportsAnUnmetPlatform()
    {
        // Arrange
        var endpoints = new[] { CreateEndpoint($"{DefaultHost}/webhooks/stripe?v=1&p=DrunkenKnitwits") };

        // Act
        var result = StripeWebhookAudit.Audit(CreatePaymentAccount(), endpoints, CreateExpectations());

        // Assert
        var check = Check(result, StripeWebhookCheckType.Platform);
        check.State.Should().Be(StripeWebhookCheckState.Unmet);
        check.Severity.Should().Be(StripeWebhookCheckSeverity.Error);
    }

    [Test]
    public static void Audit_WhenDrunkenKnitwitsUrlNamesNoPlatform_ReportsAWarning()
    {
        /* Arrange - the controller reads an absent p as Drunken Knitwits, so this endpoint works, on a
           default nothing about it states. */
        var paymentSettings = CreatePaymentAccount(platform: PlatformType.DrunkenKnitwits);
        var endpoints = new[] { CreateEndpoint($"{DefaultHost}/webhooks/stripe?v=1") };
        var expectations = CreateExpectations(hosts: Hosts(EnvironmentType.Prod, PlatformType.DrunkenKnitwits));

        // Act
        var result = StripeWebhookAudit.Audit(paymentSettings, endpoints, expectations);

        // Assert
        var check = Check(result, StripeWebhookCheckType.Platform);
        check.State.Should().Be(StripeWebhookCheckState.Unmet);
        check.Severity.Should().Be(StripeWebhookCheckSeverity.Warning);
    }

    [Test]
    public static void Audit_WhenAnotherPlatformsUrlNamesNoPlatform_ReportsAnError()
    {
        // Arrange
        var endpoints = new[] { CreateEndpoint($"{DefaultHost}/webhooks/stripe?v=1") };

        // Act
        var result = StripeWebhookAudit.Audit(CreatePaymentAccount(), endpoints, CreateExpectations());

        // Assert
        var check = Check(result, StripeWebhookCheckType.Platform);
        check.State.Should().Be(StripeWebhookCheckState.Unmet);
        check.Severity.Should().Be(StripeWebhookCheckSeverity.Error);
    }

    [Test]
    public static void Audit_WhenUrlCarriesAnUnknownParameter_ReportsIt()
    {
        // Arrange
        var endpoints = new[] { CreateEndpoint($"{DefaultHost}/webhooks/stripe?v=1&p=Default&stale=1") };

        // Act
        var result = StripeWebhookAudit.Audit(CreatePaymentAccount(), endpoints, CreateExpectations());

        // Assert
        var check = Check(result, StripeWebhookCheckType.Query);
        check.State.Should().Be(StripeWebhookCheckState.Unmet);
        check.Actual.Should().Be("stale");
    }

    [Test]
    public static void Audit_WhenUrlIsOnAnotherHost_ReportsAnUnmetHost()
    {
        // Arrange
        var endpoints = new[] { CreateEndpoint("https://elsewhere.com/webhooks/stripe?v=1&p=Default") };

        // Act
        var result = StripeWebhookAudit.Audit(CreatePaymentAccount(), endpoints, CreateExpectations());

        // Assert
        var check = Check(result, StripeWebhookCheckType.Host);
        check.State.Should().Be(StripeWebhookCheckState.Unmet);
        check.Actual.Should().Be("https://elsewhere.com");
        check.Expected.Should().Be(DefaultHost);
    }

    [TestCase("")]
    [TestCase("   ")]
    public static void Audit_WhenTheHostIsWithheld_ComparesNoHost(string host)
    {
        // Arrange
        var expectations = CreateExpectations(hosts: Hosts(EnvironmentType.Prod, PlatformType.Default, host));

        // Act
        var result = StripeWebhookAudit.Audit(CreatePaymentAccount(), [CreateEndpoint()], expectations);

        // Assert
        Check(result, StripeWebhookCheckType.Host).State.Should().Be(StripeWebhookCheckState.NotComparable);
    }

    [Test]
    public static void Audit_WhenTheEnvironmentHasNoHostAtAll_ComparesNoHost()
    {
        // Arrange
        var paymentSettings = CreatePaymentAccount(environment: EnvironmentType.E2E);

        // Act
        var result = StripeWebhookAudit.Audit(paymentSettings, [CreateEndpoint()], CreateExpectations());

        // Assert
        Check(result, StripeWebhookCheckType.Host).State.Should().Be(StripeWebhookCheckState.NotComparable);
    }

    [Test]
    public static void Audit_WhenEndpointIsDisabled_ListsItSeparatelyAndChecksItAgainstNothing()
    {
        /* Arrange - a disabled endpoint is one somebody switched off, so reporting findings against it
           would be reporting against a decision. Its URL is wrong in every way a check would catch. */
        var endpoints = new[]
        {
            CreateEndpoint(Url(StripeWebhookKind.Site)),
            CreateEndpoint(Url(StripeWebhookKind.ConnectedAccount), id: "we_2"),
            CreateEndpoint("https://elsewhere.com/webhooks/legacy", enabled: false, id: "we_old")
        };

        // Act
        var result = StripeWebhookAudit.Audit(CreatePaymentAccount(), endpoints, CreateExpectations());

        // Assert
        result.Endpoints.Select(x => x.Endpoint.Id).Should().NotContain("we_old");
        result.DisabledEndpoints.Select(x => x.Endpoint.Id).Should().Equal(["we_old"]);
        result.DisabledEndpoints.Single().Checks.Should().BeEmpty();
        result.DuplicateKinds.Should().BeEmpty();
        result.MissingKinds.Should().BeEmpty();
    }

    [Test]
    public static void Audit_WhenADisabledEndpointSupersedesAnother_ReportsNoDuplicate()
    {
        // Arrange - the case a switched-off endpoint left behind in Stripe would otherwise fabricate.
        var endpoints = new[]
        {
            CreateEndpoint(Url(StripeWebhookKind.Site)),
            CreateEndpoint(Url(StripeWebhookKind.Site), enabled: false, id: "we_old")
        };

        // Act
        var result = StripeWebhookAudit.Audit(CreatePaymentAccount(), endpoints, CreateExpectations());

        // Assert
        result.DuplicateKinds.Should().BeEmpty();
    }

    [Test]
    public static void Audit_WhenAKindsOnlyEndpointIsDisabled_ReportsTheKindMissing()
    {
        /* Arrange - disabling the one endpoint for a kind stops those events arriving, so it has to read as
           the kind having none rather than as an endpoint that exists and does nothing. */
        var endpoints = new[]
        {
            CreateEndpoint(Url(StripeWebhookKind.Site)),
            CreateEndpoint(Url(StripeWebhookKind.ConnectedAccount), enabled: false, id: "we_2")
        };

        // Act
        var result = StripeWebhookAudit.Audit(CreatePaymentAccount(), endpoints, CreateExpectations());

        // Assert
        result.MissingKinds.Should().BeEquivalentTo([StripeWebhookKind.ConnectedAccount]);
    }

    [Test]
    public static void Audit_WhenADisabledEndpointPinsAnotherApiVersion_ReportsNoDrift()
    {
        // Arrange
        var endpoints = new[]
        {
            CreateEndpoint(Url(StripeWebhookKind.Site)),
            CreateEndpoint(Url(StripeWebhookKind.ConnectedAccount), apiVersion: "2019-01-01", enabled: false, id: "we_2")
        };

        // Act
        var result = StripeWebhookAudit.Audit(CreatePaymentAccount(), endpoints, CreateExpectations());

        // Assert
        result.MixedApiVersions.Should().BeFalse();
    }

    [TestCase(EnvironmentType.Prod, true, StripeWebhookCheckState.Met)]
    [TestCase(EnvironmentType.Prod, false, StripeWebhookCheckState.Unmet)]
    [TestCase(EnvironmentType.Dev, false, StripeWebhookCheckState.Met)]
    [TestCase(EnvironmentType.Dev, true, StripeWebhookCheckState.Unmet)]
    [TestCase(EnvironmentType.E2E, true, StripeWebhookCheckState.Unmet)]
    public static void Audit_ComparesLiveModeAgainstTheEnvironment(
        EnvironmentType environment,
        bool liveMode,
        StripeWebhookCheckState expected)
    {
        // Arrange
        var paymentSettings = CreatePaymentAccount(environment: environment);
        var endpoints = new[] { CreateEndpoint(liveMode: liveMode) };

        // Act
        var result = StripeWebhookAudit.Audit(paymentSettings, endpoints, CreateExpectations());

        // Assert
        Check(result, StripeWebhookCheckType.LiveMode).State.Should().Be(expected);
    }

    [Test]
    public static void Audit_WhenEndpointsPinDifferentApiVersions_ReportsTheDrift()
    {
        // Arrange
        var endpoints = new[]
        {
            CreateEndpoint(Url(StripeWebhookKind.Site), apiVersion: "2025-01-01"),
            CreateEndpoint(Url(StripeWebhookKind.ConnectedAccount), apiVersion: null, id: "we_2")
        };

        // Act
        var result = StripeWebhookAudit.Audit(CreatePaymentAccount(), endpoints, CreateExpectations());

        // Assert
        result.MixedApiVersions.Should().BeTrue();
    }

    [Test]
    public static void Audit_WhenEndpointsShareAnApiVersion_ReportsNoDrift()
    {
        // Arrange
        var endpoints = new[]
        {
            CreateEndpoint(Url(StripeWebhookKind.Site)),
            CreateEndpoint(Url(StripeWebhookKind.ConnectedAccount), id: "we_2")
        };

        // Act
        var result = StripeWebhookAudit.Audit(CreatePaymentAccount(), endpoints, CreateExpectations());

        // Assert
        result.MixedApiVersions.Should().BeFalse();
    }

    [Test]
    public static void Audit_OrdersEndpointsByKindWithTheUnroutableOnesLast()
    {
        // Arrange
        var endpoints = new[]
        {
            CreateEndpoint($"{DefaultHost}/webhooks/stripe?p=Default", id: "we_orphan"),
            CreateEndpoint(Url(StripeWebhookKind.ConnectedAccount), id: "we_connected"),
            CreateEndpoint(Url(StripeWebhookKind.Site), id: "we_site")
        };

        // Act
        var result = StripeWebhookAudit.Audit(CreatePaymentAccount(), endpoints, CreateExpectations());

        // Assert
        result.Endpoints
            .Select(x => x.Endpoint.Id)
            .Should()
            .Equal(["we_site", "we_connected", "we_orphan"]);
    }

    private static StripeWebhookCheck Check(StripeWebhookAuditResult result, StripeWebhookCheckType type)
        => result.Endpoints.Single().Checks.Single(x => x.Type == type);

    private static StripeWebhookEndpoint CreateEndpoint(
        string? url = null,
        IReadOnlyCollection<string>? events = null,
        bool enabled = true,
        bool liveMode = true,
        string? apiVersion = "2025-01-01",
        string id = "we_1")
        => new()
        {
            ApiVersion = apiVersion,
            Description = null,
            Enabled = enabled,
            Events = events ?? ExpectedEvents,
            Id = id,
            LiveMode = liveMode,
            Url = url ?? Url(StripeWebhookKind.Site)
        };

    private static StripeWebhookAdminServiceSettings CreateExpectations(
        IReadOnlyCollection<string>? events = null,
        IReadOnlyDictionary<EnvironmentType, IReadOnlyDictionary<PlatformType, string>>? hosts = null,
        string path = "/webhooks/stripe")
        => new()
        {
            Events = events ?? ExpectedEvents,
            Hosts = hosts ?? Hosts(EnvironmentType.Prod, PlatformType.Default),
            LiveDashboardUrlFormat = "https://dashboard.stripe.com/{account}/webhooks/{id}",
            Path = path,
            TestDashboardUrlFormat = "https://dashboard.stripe.com/{account}/test/webhooks/{id}"
        };

    private static StripePaymentAccount CreatePaymentAccount(
        EnvironmentType environment = EnvironmentType.Prod,
        PlatformType platform = PlatformType.Default)
        => new()
        {
            AccountId = "acct_1",
            Environment = environment,
            Platform = platform
        };

    private static IReadOnlyDictionary<EnvironmentType, IReadOnlyDictionary<PlatformType, string>> Hosts(
        EnvironmentType environment,
        PlatformType platform,
        string host = DefaultHost)
        => new Dictionary<EnvironmentType, IReadOnlyDictionary<PlatformType, string>>
        {
            [environment] = new Dictionary<PlatformType, string>
            {
                [platform] = host
            }
        };

    private static string Url(StripeWebhookKind kind)
        => $"{DefaultHost}/webhooks/stripe?v={(int)kind}&p=Default";
}
