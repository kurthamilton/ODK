using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using ODK.Services.Integrations.Places;
using ODK.Services.Logging;

namespace ODK.Services.Integrations.Tests.Places;

[Parallelizable]
public static class PlacesServiceTests
{
    /* Trimmed from a real Places API (New) response for the field mask the service asks for. The shape is
       the point: displayName is an object with a text property, location is latitude/longitude rather than
       lat/lng, and an address component carries longText rather than long_name. */
    private const string OakJson =
        """
        {
          "id": "ChIJN1t_tDeuEmsRUsoyG83frY4",
          "formattedAddress": "123 High St, Sheffield S1 2AB, UK",
          "location": { "latitude": 53.3811, "longitude": -1.4701 },
          "displayName": { "text": "The Oak", "languageCode": "en" },
          "addressComponents": [
            { "longText": "123", "shortText": "123", "types": [ "street_number" ] },
            { "longText": "Sheffield", "shortText": "Sheffield", "types": [ "postal_town" ] },
            { "longText": "England", "shortText": "England", "types": [ "administrative_area_level_1" ] }
          ]
        }
        """;

    [Test]
    public static async Task GetPlace_ApiKeyNotConfigured_Fails()
    {
        // Arrange - a blank key is unconfigured, not a key that happens to be empty.
        var (service, _) = CreateService(HttpStatusCode.OK, OakJson, apiKey: string.Empty);

        // Act
        var result = await service.GetPlace("ChIJN1t_tDeuEmsRUsoyG83frY4");

        // Assert
        result.Success.Should().BeFalse();
        result.NotFound.Should().BeFalse();
        result.Place.Should().BeNull();
    }

    [Test]
    public static async Task GetPlace_IncompleteResponse_Fails()
    {
        // Arrange - a place with no position cannot be recorded; taking it would put a venue at 0,0.
        var json = """{ "id": "abc", "displayName": { "text": "The Oak" } }""";
        var (service, _) = CreateService(HttpStatusCode.OK, json);

        // Act
        var result = await service.GetPlace("abc");

        // Assert
        result.Success.Should().BeFalse();
        result.NotFound.Should().BeFalse();
    }

    [Test]
    public static async Task GetPlace_LocalityOnly_UsesLocality()
    {
        // Arrange - most countries return a locality and no postal town.
        var json =
            """
            {
              "id": "abc",
              "location": { "latitude": 1, "longitude": 2 },
              "displayName": { "text": "Cafe" },
              "addressComponents": [ { "longText": "Lyon", "types": [ "locality" ] } ]
            }
            """;
        var (service, _) = CreateService(HttpStatusCode.OK, json);

        // Act
        var result = await service.GetPlace("abc");

        // Assert
        result.Place!.Locality.Should().Be("Lyon");
    }

    [Test]
    public static async Task GetPlace_NoLocality_ReturnsNullLocality()
    {
        // Arrange - somewhere too remote to sit inside a town.
        var json =
            """
            {
              "id": "abc",
              "location": { "latitude": 1, "longitude": 2 },
              "displayName": { "text": "Bothy" },
              "addressComponents": [ { "longText": "Scotland", "types": [ "administrative_area_level_1" ] } ]
            }
            """;
        var (service, _) = CreateService(HttpStatusCode.OK, json);

        // Act
        var result = await service.GetPlace("abc");

        // Assert
        result.Success.Should().BeTrue();
        result.Place!.Locality.Should().BeNull();
    }

    [Test]
    public static async Task GetPlace_NotFound_IsItsOwnOutcome()
    {
        // Arrange - a place ID stops resolving when the place closes, moves or is merged.
        var (service, _) = CreateService(HttpStatusCode.NotFound, """{ "error": { "code": 404 } }""");

        // Act
        var result = await service.GetPlace("ChIJstale");

        // Assert - distinguishable from a failure, because the member can act on it.
        result.Success.Should().BeFalse();
        result.NotFound.Should().BeTrue();
    }

    [Test]
    public static async Task GetPlace_ExternalIdEmpty_FailsWithoutCallingGoogle()
    {
        // Arrange
        var (service, handler) = CreateService(HttpStatusCode.OK, OakJson);

        // Act
        var result = await service.GetPlace(string.Empty);

        // Assert
        result.Success.Should().BeFalse();
        handler.Request.Should().BeNull();
    }

    [Test]
    public static async Task GetPlace_RequestThrows_Fails()
    {
        // Arrange
        var (service, _) = CreateService(new HttpRequestException("connection reset"));

        // Act
        var result = await service.GetPlace("abc");

        // Assert - an exception is ours to deal with, not the member's.
        result.Success.Should().BeFalse();
        result.NotFound.Should().BeFalse();
    }

    [Test]
    public static async Task GetPlace_SendsTheIdKeyAndFieldMask()
    {
        // Arrange
        var (service, handler) = CreateService(HttpStatusCode.OK, OakJson, apiKey: "test-key");

        // Act
        await service.GetPlace("ChIJN1t_tDeuEmsRUsoyG83frY4");

        // Assert
        var request = handler.Request!;
        request.RequestUri!.AbsoluteUri
            .Should().Be("https://places.googleapis.com/v1/places/ChIJN1t_tDeuEmsRUsoyG83frY4");
        request.Headers.GetValues("X-Goog-Api-Key").Single().Should().Be("test-key");
        request.Headers.GetValues("X-Goog-FieldMask").Single()
            .Should().Be("id,displayName,location,addressComponents,formattedAddress");
    }

    [Test]
    public static async Task GetPlace_Success_ReadsThePlace()
    {
        // Arrange
        var (service, _) = CreateService(HttpStatusCode.OK, OakJson);

        // Act
        var result = await service.GetPlace("ChIJN1t_tDeuEmsRUsoyG83frY4");

        // Assert
        result.Success.Should().BeTrue();
        var place = result.Place!;
        place.ExternalId.Should().Be("ChIJN1t_tDeuEmsRUsoyG83frY4");
        place.Name.Should().Be("The Oak");
        place.Location.Lat.Should().Be(53.3811);
        place.Location.Long.Should().Be(-1.4701);
        place.FormattedAddress.Should().Be("123 High St, Sheffield S1 2AB, UK");
        place.Locality.Should().Be("Sheffield");
    }

    private static (PlacesService Service, TestHttpMessageHandler Handler) CreateService(
        HttpStatusCode statusCode, string content, string apiKey = "test-key")
        => CreateService(new TestHttpMessageHandler(statusCode, content), apiKey);

    private static (PlacesService Service, TestHttpMessageHandler Handler) CreateService(Exception exception)
        => CreateService(new TestHttpMessageHandler(exception), "test-key");

    private static (PlacesService Service, TestHttpMessageHandler Handler) CreateService(
        TestHttpMessageHandler handler, string apiKey)
    {
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(handler));

        var service = new PlacesService(
            new PlacesServiceSettings { ApiKey = apiKey },
            httpClientFactory.Object,
            new Mock<ILoggingService>().Object);

        return (service, handler);
    }
}
