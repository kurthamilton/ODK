using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using NUnit.Framework;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Services;
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
            Metadata = new Dictionary<string, string> { ["reason"] = "chapterSubscription" },
            OriginatedUtc = new DateTime(2026, 8, 20, 9, 30, 0, DateTimeKind.Utc),
            PaymentId = "pi_1",
            PaymentProviderType = PaymentProviderType.Stripe,
            SubscriptionId = "sub_1",
            Type = PaymentProviderWebhookType.CheckoutSessionCompleted
        };

        // Act
        var result = SerializationHelper.Deserialize<PaymentProviderWebhook>(
            SerializationHelper.Serialize(webhook, SerializationOption.User), SerializationOption.User);

        // Assert
        result.Amount.Should().Be(webhook.Amount);
        result.Id.Should().Be(webhook.Id);
        result.Metadata.Should().BeEquivalentTo(webhook.Metadata);
        result.OriginatedUtc.Should().Be(webhook.OriginatedUtc);
        result.PaymentProviderType.Should().Be(webhook.PaymentProviderType);
        result.Type.Should().Be(webhook.Type);
    }

    [Test]
    public static void LegacyServiceRequestPayload_StillDeserialises()
    {
        /* Arrange - a payload in the shape queued before jobs took a JobRequest, captured verbatim. Jobs
           holding one are still in the queue and still bind to the work methods that take an IServiceRequest,
           so ServiceRequest and HttpRequestContext have to keep deserialising until the queue has drained of
           them. When that is done, this test and those methods go together. */
        var json =
            "{\"$type\":\"ODK.Services.ServiceRequest, ODK.Services\"," +
            "\"CurrentMemberOrDefault\":null," +
            "\"HttpRequestContext\":{" +
            "\"$type\":\"ODK.Web.Razor.Services.HttpRequestContext, ODK.Web.Razor\"," +
            "\"Headers\":{},\"IpAddress\":\"203.0.113.9\",\"Locale\":\"en-GB\"," +
            "\"RequestPath\":\"/events\",\"RequestUrl\":\"https://example.com/events\"," +
            "\"RouteValues\":{},\"UserAgent\":\"probe\"}," +
            "\"Platform\":2}";

        // Act
        var result = SerializationHelper.Deserialize<IServiceRequest>(json, SerializationOption.User);

        // Assert
        result.Platform.Should().Be(PlatformType.DrunkenKnitwits);
        result.HttpRequestContext.BaseUrl.Should().Be("https://example.com");
    }

    private static JobRequest CreateJobRequest() => new()
    {
        BaseUrl = "https://example.com",
        ChapterId = ChapterId,
        CurrentMemberId = MemberId,
        Platform = PlatformType.DrunkenKnitwits
    };
}
