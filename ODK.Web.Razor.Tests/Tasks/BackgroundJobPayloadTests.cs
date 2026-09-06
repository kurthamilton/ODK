using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using NUnit.Framework;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Services.Payments.Models;
using ODK.Services.Tasks;

namespace ODK.Web.Razor.Tests.Tasks;

/// <summary>
/// Pins the JSON a job's arguments are stored as.
/// </summary>
/// <remarks>
/// The queue holds these strings across deploys, so a property renamed, retyped or removed silently breaks
/// every job already holding one. The pinned document catches those. It does not catch an added property,
/// because a null one is omitted from the JSON entirely - the property set is asserted separately for that,
/// since adding to a format other deploys are reading is worth stating out loud even though old payloads
/// survive it.
/// </remarks>
[Parallelizable]
public static class BackgroundJobPayloadTests
{
    private static readonly Guid ChapterId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid MemberId = new("22222222-2222-2222-2222-222222222222");

    [OneTimeSetUp]
    public static void ConfigureSerializer()
        => GlobalConfiguration.Configuration
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings();

    [Test]
    public static void JobRequest_SerialisesToItsApprovedShape()
    {
        // Arrange
        var request = CreateJobRequest();

        // Act
        var json = SerializationHelper.Serialize(request, SerializationOption.User);

        /* Assert - and note there is no $type. The job methods declare this concrete sealed type rather than
           an interface, so nothing writes an assembly-qualified name into the payload, and moving the type
           cannot break a job already holding one. */
        json.Should().Be(
            "{" +
            "\"BaseUrl\":\"https://example.com\"," +
            "\"ChapterId\":\"11111111-1111-1111-1111-111111111111\"," +
            "\"CurrentMemberId\":\"22222222-2222-2222-2222-222222222222\"," +
            "\"Platform\":2" +
            "}");
    }

    [Test]
    public static void JobRequest_HasOnlyItsApprovedProperties()
    {
        /* Arrange - an added property is invisible to the pinned document above while its value is null, and
           null is what every job queued before it was added will supply. */
        var expected = new[] { "BaseUrl", "ChapterId", "CurrentMemberId", "Platform" };

        // Act
        var properties = typeof(JobRequest).GetProperties().Select(x => x.Name);

        // Assert
        properties.Should().BeEquivalentTo(expected);
    }

    [Test]
    public static void JobRequest_RoundTrips()
    {
        // Arrange
        var request = CreateJobRequest();

        // Act
        var result = SerializationHelper.Deserialize<JobRequest>(
            SerializationHelper.Serialize(request, SerializationOption.User), SerializationOption.User);

        // Assert
        result.BaseUrl.Should().Be(request.BaseUrl);
        result.ChapterId.Should().Be(request.ChapterId);
        result.CurrentMemberId.Should().Be(request.CurrentMemberId);
        result.Platform.Should().Be(request.Platform);
    }

    [Test]
    public static void JobRequest_NullIds_RoundTrip()
    {
        // Arrange - a job about no group, queued by nobody signed in.
        var request = new JobRequest
        {
            BaseUrl = "https://example.com",
            ChapterId = null,
            CurrentMemberId = null,
            Platform = PlatformType.Default
        };

        // Act
        var result = SerializationHelper.Deserialize<JobRequest>(
            SerializationHelper.Serialize(request, SerializationOption.User), SerializationOption.User);

        // Assert
        result.ChapterId.Should().BeNull();
        result.CurrentMemberId.Should().BeNull();
    }

    [Test]
    public static void PaymentProviderWebhook_RoundTrips()
    {
        // Arrange - the other argument a job carries that is not a primitive.
        var webhook = new PaymentProviderWebhook
        {
            Amount = 12.34m,
            Complete = true,
            Id = "wh_1",
            InvoiceId = "in_1",
            Metadata = new Dictionary<string, string> { ["reason"] = "chapterSubscription" },
            OriginatedUtc = new DateTime(2026, 8, 20, 9, 30, 0, DateTimeKind.Utc),
            PaymentId = "pi_1",
            PaymentProviderType = PaymentProviderType.Stripe,
            SubscriptionId = "sub_1",
            SubscriptionRenewal = true,
            Type = PaymentProviderWebhookType.CheckoutSessionCompleted
        };

        // Act
        var result = SerializationHelper.Deserialize<PaymentProviderWebhook>(
            SerializationHelper.Serialize(webhook, SerializationOption.User), SerializationOption.User);

        // Assert
        result.Amount.Should().Be(webhook.Amount);
        result.Id.Should().Be(webhook.Id);
        result.InvoiceId.Should().Be(webhook.InvoiceId);
        result.Metadata.Should().BeEquivalentTo(webhook.Metadata);
        result.OriginatedUtc.Should().Be(webhook.OriginatedUtc);
        result.PaymentProviderType.Should().Be(webhook.PaymentProviderType);
        result.SubscriptionRenewal.Should().Be(webhook.SubscriptionRenewal);
        result.Type.Should().Be(webhook.Type);
    }

    [Test]
    public static void PaymentProviderWebhook_PayloadWithoutInvoiceId_Deserialises()
    {
        /* Arrange - what a job queued before InvoiceId existed holds. The property is `required`, which the
           compiler enforces and the serialiser does not, so an absent one has to arrive as null rather than
           throw - otherwise every webhook in the queue at deploy time is lost. */
        const string json =
            "{\"Amount\":12.34,\"Complete\":true,\"Id\":\"wh_1\",\"Metadata\":{}," +
            "\"OriginatedUtc\":\"2026-08-20T09:30:00Z\",\"PaymentId\":\"pi_1\"," +
            "\"PaymentProviderType\":1,\"SubscriptionId\":null,\"Type\":1}";

        // Act
        var result = SerializationHelper.Deserialize<PaymentProviderWebhook>(json, SerializationOption.User);

        // Assert
        result.InvoiceId.Should().BeNull();
        result.PaymentId.Should().Be("pi_1");
    }

    [Test]
    public static void PaymentProviderWebhook_PayloadWithoutSubscriptionRenewal_DeserialisesAsNotARenewal()
    {
        /* Arrange - what a job queued before the flag existed holds. False is the safe way for one to
           arrive: an event no longer able to say it renewed a subscription announces nothing, rather
           than telling a member their first payment was a renewal. */
        const string json =
            "{\"Amount\":12.34,\"Complete\":true,\"Id\":\"wh_1\",\"InvoiceId\":\"in_1\",\"Metadata\":{}," +
            "\"OriginatedUtc\":\"2026-08-20T09:30:00Z\",\"PaymentId\":\"pi_1\"," +
            "\"PaymentProviderType\":1,\"SubscriptionId\":\"sub_1\",\"Type\":3}";

        // Act
        var result = SerializationHelper.Deserialize<PaymentProviderWebhook>(json, SerializationOption.User);

        // Assert
        result.SubscriptionRenewal.Should().BeFalse();
        result.SubscriptionId.Should().Be("sub_1");
    }

    private static JobRequest CreateJobRequest() => new()
    {
        BaseUrl = "https://example.com",
        ChapterId = ChapterId,
        CurrentMemberId = MemberId,
        Platform = PlatformType.DrunkenKnitwits
    };
}
