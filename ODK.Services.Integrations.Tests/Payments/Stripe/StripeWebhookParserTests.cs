using FluentAssertions;
using Moq;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Services.Exceptions;
using ODK.Services.Integrations.Payments.Stripe;
using ODK.Services.Logging;
using ODK.Services.Payments.Models;
using Stripe;

namespace ODK.Services.Integrations.Tests.Payments.Stripe;

[Parallelizable]
public static class StripeWebhookParserTests
{
    private const long CreatedUnix = 1767225600;

    private const string EventId = "evt_123";

    private const string Secret = "whsec_test";

    private static readonly DateTime CreatedUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Test]
    public static async Task ParseWebhook_BlankSecret_Throws()
    {
        // Arrange
        var json = CreateEventJson(EventTypes.CheckoutSessionCompleted, CheckoutSessionJson());

        var parser = CreateParser(secretsV1: new Dictionary<PlatformType, string>
        {
            [PlatformType.Default] = string.Empty
        });

        // Act
        var act = async () => await ParseSigned(parser, json);

        // Assert - a secret that is configured but empty is unconfigured, not a secret to check against
        await act.Should().ThrowAsync<OdkServiceException>();
    }

    [Test]
    public static async Task ParseWebhook_CheckoutSessionCompleted_MapsSession()
    {
        // Arrange
        var json = CreateEventJson(EventTypes.CheckoutSessionCompleted, CheckoutSessionJson());

        // Act
        var result = await ParseSigned(CreateParser(), json);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(25);
        result.Complete.Should().BeTrue();
        result.Id.Should().Be(EventId);
        result.InvoiceId.Should().BeNull();
        result.Metadata.Should().BeEquivalentTo(new Dictionary<string, string> { ["memberId"] = "member-1" });
        result.OriginatedUtc.Should().Be(CreatedUtc);
        result.PaymentId.Should().Be("pi_123");
        result.PaymentProviderType.Should().Be(PaymentProviderType.Stripe);
        result.SubscriptionId.Should().BeNull();
        result.Type.Should().Be(PaymentProviderWebhookType.CheckoutSessionCompleted);
    }

    [Test]
    public static async Task ParseWebhook_CheckoutSessionCompleted_Unpaid_NotComplete()
    {
        // Arrange
        var json = CreateEventJson(
            EventTypes.CheckoutSessionCompleted,
            CheckoutSessionJson(paymentStatus: "unpaid"));

        // Act
        var result = await ParseSigned(CreateParser(), json);

        // Assert
        result.Should().NotBeNull();
        result.Complete.Should().BeFalse();
    }

    [Test]
    public static async Task ParseWebhook_CheckoutSessionExpired_MapsSession()
    {
        // Arrange
        var json = CreateEventJson(
            EventTypes.CheckoutSessionExpired,
            CheckoutSessionJson(paymentStatus: "unpaid", status: "expired"));

        // Act
        var result = await ParseSigned(CreateParser(), json);

        // Assert - an expired session took no money, so the amount is zero whatever the session totalled
        result.Should().NotBeNull();
        result.Amount.Should().Be(0);
        result.Complete.Should().BeTrue();
        result.Id.Should().Be(EventId);
        result.InvoiceId.Should().BeNull();
        result.Metadata.Should().BeEquivalentTo(new Dictionary<string, string> { ["memberId"] = "member-1" });
        result.OriginatedUtc.Should().Be(CreatedUtc);
        result.PaymentId.Should().Be("pi_123");
        result.SubscriptionId.Should().BeNull();
        result.Type.Should().Be(PaymentProviderWebhookType.CheckoutSessionExpired);
    }

    [Test]
    public static async Task ParseWebhook_CustomerSubscriptionDeleted_MapsSubscription()
    {
        // Arrange
        var json = CreateEventJson(EventTypes.CustomerSubscriptionDeleted, SubscriptionJson());

        // Act
        var result = await ParseSigned(CreateParser(), json);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(0);
        result.Complete.Should().BeTrue();
        result.Id.Should().Be(EventId);
        result.InvoiceId.Should().BeNull();
        result.Metadata.Should().BeEquivalentTo(new Dictionary<string, string> { ["memberId"] = "member-1" });
        result.OriginatedUtc.Should().Be(CreatedUtc);
        result.PaymentId.Should().BeNull();
        result.SubscriptionId.Should().Be("sub_123");
        result.Type.Should().Be(PaymentProviderWebhookType.SubscriptionCancelled);
    }

    [Test]
    public static async Task ParseWebhook_InvalidSignature_ReturnsNullAndLogsError()
    {
        // Arrange
        var json = CreateEventJson(EventTypes.CheckoutSessionCompleted, CheckoutSessionJson());

        var loggingService = new Mock<ILoggingService>();

        // Act
        var result = await ParseSigned(CreateParser(loggingService.Object), json, secret: "whsec_other");

        // Assert - swallowing is deliberate: a payload that fails validation is logged, not re-delivered
        result.Should().BeNull();
        loggingService.Verify(x => x.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
    }

    [Test]
    public static async Task ParseWebhook_InvoicePaymentSucceeded_MapsInvoice()
    {
        // Arrange
        var json = CreateEventJson(EventTypes.InvoicePaymentSucceeded, InvoiceJson());

        // Act
        var result = await ParseSigned(CreateParser(), json);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(12);
        result.Complete.Should().BeTrue();
        result.Id.Should().Be(EventId);
        result.Metadata.Should().BeEquivalentTo(new Dictionary<string, string> { ["memberId"] = "member-1" });
        result.InvoiceId.Should().Be("in_123");
        result.OriginatedUtc.Should().Be(CreatedUtc);
        // An invoice names no payment - the invoice id is the handle on what it charged
        result.PaymentId.Should().BeNull();
        result.SubscriptionId.Should().Be("sub_123");
        result.Type.Should().Be(PaymentProviderWebhookType.InvoicePaymentSucceeded);
    }

    [TestCase("subscription_cycle", true)]
    [TestCase("subscription_update", true)]
    [TestCase("", true)]
    [TestCase("subscription_create", false)]
    public static async Task ParseWebhook_InvoicePaymentSucceeded_ReadsRenewalFromBillingReason(
        string billingReason, bool expected)
    {
        // Arrange - only the reason that says an invoice created its subscription is not a renewal;
        // anything else, an unrecognised reason included, is a later billing of one.
        var json = CreateEventJson(EventTypes.InvoicePaymentSucceeded, InvoiceJson(billingReason));

        // Act
        var result = await ParseSigned(CreateParser(), json);

        // Assert
        result.Should().NotBeNull();
        result.SubscriptionRenewal.Should().Be(expected);
    }

    [Test]
    public static async Task ParseWebhook_InvoicePaymentSucceeded_NoSubscriptionDetails_EmptyMetadata()
    {
        // Arrange - an invoice not tied to a subscription carries no subscription details at all
        var json = CreateEventJson(EventTypes.InvoicePaymentSucceeded, InvoiceWithNoSubscriptionDetailsJson());

        // Act
        var result = await ParseSigned(CreateParser(), json);

        // Assert
        result.Should().NotBeNull();
        result.Metadata.Should().BeEmpty();
        result.SubscriptionId.Should().BeNull();
        // Nothing renewed: an invoice billed by no subscription cannot be a later billing of one.
        result.SubscriptionRenewal.Should().BeFalse();
    }

    [Test]
    public static async Task ParseWebhook_SecretNotSetForPlatform_Throws()
    {
        // Arrange
        var json = CreateEventJson(EventTypes.CheckoutSessionCompleted, CheckoutSessionJson());

        var parser = CreateParser(secretsV1: new Dictionary<PlatformType, string>());

        // Act
        var act = async () => await ParseSigned(parser, json);

        // Assert - throwing returns a 5xx, so Stripe re-delivers once the secret is configured
        await act.Should().ThrowAsync<OdkServiceException>();
    }

    [Test]
    public static async Task ParseWebhook_UnhandledEventType_ReturnsNull()
    {
        // Arrange
        var json = CreateEventJson(
            EventTypes.PaymentIntentSucceeded,
            """{ "id": "pi_123", "object": "payment_intent" }""");

        // Act
        var result = await ParseSigned(CreateParser(), json);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public static async Task ParseWebhook_Version2_UsesV2Secret()
    {
        // Arrange
        var json = CreateEventJson(EventTypes.CheckoutSessionCompleted, CheckoutSessionJson());

        var parser = CreateParser(
            secretsV1: new Dictionary<PlatformType, string> { [PlatformType.Default] = "whsec_v1" },
            secretsV2: new Dictionary<PlatformType, string> { [PlatformType.Default] = "whsec_v2" });

        // Act
        var signedWithV2 = await ParseSigned(parser, json, secret: "whsec_v2", version: 2);
        var signedWithV1 = await ParseSigned(parser, json, secret: "whsec_v1", version: 2);

        // Assert
        signedWithV2.Should().NotBeNull();
        signedWithV1.Should().BeNull();
    }

    private static string CheckoutSessionJson(string paymentStatus = "paid", string status = "complete") =>
        $$"""
        {
          "id": "cs_123",
          "object": "checkout.session",
          "amount_total": 2500,
          "metadata": { "memberId": "member-1" },
          "payment_intent": "pi_123",
          "payment_status": "{{paymentStatus}}",
          "status": "{{status}}"
        }
        """;

    private static string CreateEventJson(string type, string dataObjectJson) =>
        $$"""
        {
          "id": "{{EventId}}",
          "object": "event",
          "api_version": "{{StripeConfiguration.ApiVersion}}",
          "created": {{CreatedUnix}},
          "type": "{{type}}",
          "data": { "object": {{dataObjectJson}} }
        }
        """;

    private static StripeWebhookParser CreateParser(
        ILoggingService? loggingService = null,
        IReadOnlyDictionary<PlatformType, string>? secretsV1 = null,
        IReadOnlyDictionary<PlatformType, string>? secretsV2 = null)
        => new StripeWebhookParser(
            loggingService ?? new Mock<ILoggingService>().Object,
            new StripeWebhookParserSettings
            {
                WebhookSecretsV1 = secretsV1 ?? new Dictionary<PlatformType, string>
                {
                    [PlatformType.Default] = Secret
                },
                WebhookSecretsV2 = secretsV2 ?? new Dictionary<PlatformType, string>
                {
                    [PlatformType.Default] = Secret
                }
            });

    private static string InvoiceJson(string billingReason = "subscription_cycle") =>
        $$"""
        {
          "id": "in_123",
          "object": "invoice",
          "amount_paid": 1200,
          "status": "paid",
          "billing_reason": "{{billingReason}}",
          "parent": {
            "type": "subscription_details",
            "subscription_details": {
              "subscription": "sub_123",
              "metadata": { "memberId": "member-1" }
            }
          }
        }
        """;

    private static string InvoiceWithNoSubscriptionDetailsJson() =>
        """
        {
          "id": "in_123",
          "object": "invoice",
          "amount_paid": 1200,
          "status": "paid"
        }
        """;

    private static Task<PaymentProviderWebhook?> ParseSigned(
        StripeWebhookParser parser,
        string json,
        string secret = Secret,
        int version = 1)
        => parser.ParseWebhook(
            PlatformType.Default,
            json,
            EventUtility.GenerateSignatureHeader(json, secret),
            version);

    private static string SubscriptionJson() =>
        """
        {
          "id": "sub_123",
          "object": "subscription",
          "metadata": { "memberId": "member-1" },
          "status": "canceled"
        }
        """;
}
