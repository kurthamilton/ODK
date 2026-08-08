using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using ODK.Infrastructure.Settings;

namespace ODK.Web.Razor.Tests.Settings;

/// <summary>
/// Guards the shape of the config the deploy pipeline writes, rather than any app behaviour. The pipeline
/// turns each Doppler secret into a top-level property whose name is the full config path with ':' between
/// levels (see DEPLOYMENT.md), which is an unusual shape to bind and easy to get subtly wrong — a
/// dictionary-shaped setting sourced as one-secret-per-entry loads without error and simply produces the
/// wrong keys. These tests pin the working shape so that can't regress silently.
/// </summary>
[Parallelizable]
public static class DopplerConfigShapeTests
{
    // How the base appsettings.json declares the Instagram client - empty containers that the deploy-time
    // file overrides.
    private const string BaseJson =
        """
        {
          "Instagram": {
            "BaseUrl": "https://www.instagram.com",
            "Client": { "Cookies": {}, "GraphQL": { "PostsDocId": "" }, "Headers": {} },
            "FetchWaitSeconds": 10,
            "Paths": { "Channel": "/{username}", "GraphQL": "/graphql/query", "Post": "/p/{id}", "Tag": "/t/{tag}" }
          }
        }
        """;

    [Test]
    public static void JsonObjectSecret_BindsDictionaryKeysVerbatim()
    {
        // Arrange - one secret per dictionary, value a JSON object. Underscores and casing must survive,
        // because these keys are the cookie and header names actually sent.
        var config = Build(
            """
            {
              "INSTAGRAM:CLIENT:COOKIES": { "ds_user_id": "1234567890", "sessionid": "abc", "csrftoken": "xyz" },
              "INSTAGRAM:CLIENT:HEADERS": { "x-ig-app-id": "9876", "x-csrftoken": "xyz" },
              "INSTAGRAM:CLIENT:GRAPHQL": { "PostsDocId": "1122334455" }
            }
            """);

        // Act
        var settings = config.GetSection("Instagram").Get<InstagramSettings>();

        // Assert
        settings.Should().NotBeNull();
        settings!.Client.Cookies.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            ["ds_user_id"] = "1234567890",
            ["sessionid"] = "abc",
            ["csrftoken"] = "xyz"
        });
        settings.Client.Headers.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            ["x-ig-app-id"] = "9876",
            ["x-csrftoken"] = "xyz"
        });
        settings.Client.GraphQL.PostsDocId.Should().Be("1122334455");
    }

    [Test]
    public static void PerEntrySecret_MangledByTheUnderscoreConversion()
    {
        // Arrange - what one-secret-per-cookie produces after the pipeline's unconditional '_' -> ':'
        // conversion. Documents the failure this shape causes rather than endorsing it.
        var config = Build(
            """
            {
              "INSTAGRAM:CLIENT:COOKIES:DS:USER:ID": "1234567890",
              "INSTAGRAM:CLIENT:COOKIES:SESSIONID": "abc"
            }
            """);

        // Act
        var cookies = config.GetSection("Instagram:Client:Cookies").GetChildren();

        // Assert - "ds_user_id" became a nested DS section, and the case-sensitive cookie name is upper-cased.
        cookies.Select(x => x.Key).Should().BeEquivalentTo(["DS", "SESSIONID"]);
        cookies.Should().NotContain(x => x.Key == "ds_user_id");
    }

    [Test]
    public static void PostsDocId_BindsFromEitherLeafOrObjectSecret()
    {
        // Arrange - GraphQL:PostsDocId is a plain leaf, so it works either as its own secret or nested in a
        // GRAPHQL object. Both are in use, so both need to keep working.
        var asLeaf = Build("""{ "INSTAGRAM:CLIENT:GRAPHQL:POSTSDOCID": "1122334455" }""");
        var asObject = Build("""{ "INSTAGRAM:CLIENT:GRAPHQL": { "PostsDocId": "1122334455" } }""");

        // Act / Assert - the leaf form relies on .NET matching config keys case-insensitively.
        asLeaf.GetSection("Instagram:Client:GraphQL").Get<InstagramClientGraphQLSettings>()!
            .PostsDocId.Should().Be("1122334455");
        asObject.GetSection("Instagram:Client:GraphQL").Get<InstagramClientGraphQLSettings>()!
            .PostsDocId.Should().Be("1122334455");
    }

    private static IConfigurationRoot Build(string productionJson)
        => new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(BaseJson)))
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(productionJson)))
            .Build();
}
