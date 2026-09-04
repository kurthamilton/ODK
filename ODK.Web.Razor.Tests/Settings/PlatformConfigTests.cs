using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ODK.Core.Platforms;
using ODK.Infrastructure;
using ODK.Infrastructure.Settings;
using ODK.Services.Exceptions;
using ODK.Services.Platforms;

namespace ODK.Web.Razor.Tests.Settings;

/// <summary>
/// The platform a deployment serves comes from its configuration, so what config says and what the app runs
/// as are two different things and the mapping between them is where they meet.
/// </summary>
[Parallelizable]
public static class PlatformConfigTests
{
    [Test]
    public static void AppSettings_PlatformStatesASectionLabel_FailsToBind()
    {
        /* Arrange - a label, which keys a per-platform section but is not how the platform a deployment
           serves is stated. */
        var act = () => BindAppSettings("""{ "Platform": "GS" }""");

        /* Act / Assert - naming the value and the type it could not become. The binder yields None for a
           platform config never stated, and None is served as Drunken Knitwits, so a value it could read as
           unstated would serve a platform by accident rather than fail. */
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*GS*")
            .WithMessage($"*{nameof(PlatformType)}*");
    }

    [Test]
    public static void PlatformKeyExtensions_EveryLabelAndPlatform_RoundTrips()
    {
        /* Arrange - a section is keyed by label while the platform a deployment serves is stated as the app
           spells it, so selecting an entry crosses from one vocabulary to the other and back. The two
           mappings are separate switches, and nothing but this stops them disagreeing. */
        var labels = Enum.GetValues<PlatformKey>().Where(x => x != PlatformKey.None).ToArray();
        var platforms = Enum.GetValues<PlatformType>().Where(x => x != PlatformType.None).ToArray();

        // Act / Assert - every member of each, so a platform added to one alone fails here.
        labels.Should().AllSatisfy(label => label.ToPlatformType().ToPlatformKey().Should().Be(label));
        platforms.Should().AllSatisfy(
            platform => platform.ToPlatformKey().ToPlatformType().Should().Be(platform));
    }

    [Test]
    public static void ConfigureDependencies_PlatformNotStated_ServesDrunkenKnitwits()
    {
        // Arrange - what the binder yields for a deployment whose config never named a platform.
        var appSettings = BindAppSettings("""{ "Platform": "None" }""");

        // Act
        var settings = MapPlatformProviderSettings(appSettings);

        // Assert
        settings.Platform.Should().Be(PlatformType.DrunkenKnitwits);
    }

    [Test]
    public static void ConfigureDependencies_PlatformStated_ServesThatPlatform()
    {
        // Arrange - a top-level key, which is how the deploy pipeline injects the site's PLATFORM Variable.
        var appSettings = BindAppSettings("""{ "Platform": "Default" }""");

        // Act
        var settings = MapPlatformProviderSettings(appSettings);

        // Assert
        settings.Platform.Should().Be(PlatformType.Default);
    }

    [Test]
    public static void ConfigureDependencies_SectionKeyedNone_Throws()
    {
        /* Arrange - None is a label, so unlike every other key that names no platform it binds rather than
           being dropped. It reaches the mapping, which is where it has to be refused. */
        var appSettings = BindAppSettings(
            """
            {
              "Platform": "Default",
              "Platforms": { "None": { "Name": "", "Url": "" } }
            }
            """);

        // Act
        var act = () => MapPlatformProviderSettings(appSettings);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*None*");
    }

    /* Every per-platform section has to select the same platform's entry. A deployment logging to one
       platform's directory, shipping to the other's BetterStack source and queueing to a third's Hangfire
       schema would be far harder to spot than any one of those alone, so they are asserted together rather
       than one test each. */
    [TestCase("Default", "Hangfire2", "gs")]
    [TestCase("DrunkenKnitwits", "Hangfire", "dk")]
    [TestCase("None", "Hangfire", "dk")]
    public static void ServedPlatform_SelectsOnePlatformsEntryFromEverySection(
        string configuredPlatform,
        string expectedSchemaName,
        string expectedPlatformSuffix)
    {
        // Arrange
        var appSettings = BindAppSettings(
            $$"""
            {
              "Platform": "{{configuredPlatform}}",
              "BetterStack": {
                "Platforms": {
                  "DK": { "IngestingHost": "host-dk", "SourceToken": "token-dk" },
                  "GS": { "IngestingHost": "host-gs", "SourceToken": "token-gs" }
                }
              },
              "Logging": {
                "Platforms": {
                  "DK": { "Path": "C:\\Logs\\dk" },
                  "GS": { "Path": "C:\\Logs\\gs" }
                }
              }
            }
            """);

        // Act
        var betterStack = ServedPlatform.Of(appSettings, appSettings.BetterStack.Platforms);
        var hangfire = ServedPlatform.Of(appSettings, appSettings.Hangfire.Platforms);
        var logging = ServedPlatform.Of(appSettings, appSettings.Logging.Platforms);

        // Assert - the schema names come from the committed file, which states them for every platform.
        hangfire.SchemaName.Should().Be(expectedSchemaName);
        logging.Path.Should().Be($@"C:\Logs\{expectedPlatformSuffix}");

        // A source's host and token have to come from the same entry: crossing them ships to a host that
        // rejects the token, and the sink reports that to SelfLog and nowhere else.
        betterStack.SourceToken.Should().Be($"token-{expectedPlatformSuffix}");
        betterStack.IngestingHost.Should().Be($"host-{expectedPlatformSuffix}");
    }

    [Test]
    public static void ServedPlatform_SectionOmitsThisPlatform_TakesTheGSEntry()
    {
        /* Arrange - bound from this JSON alone rather than over the committed file, which states every
           platform: layering can only override an entry, never remove one, so the committed file is what
           stops this case arising in the app. It is covered because the fallback is what makes adding a
           platform to the enum safe before every section has an entry for it. */
        var appSettings = BindSettingsWithoutBaseFile(
            """
            {
              "Platform": "DrunkenKnitwits",
              "Logging": { "Platforms": { "GS": { "Path": "C:\\Logs\\only" } } }
            }
            """);

        // Act
        var logging = ServedPlatform.Of(appSettings, appSettings.Logging.Platforms);

        // Assert - as SiteEmailSettingsProvider and IPlatformProvider.GetName do for the same case.
        logging.Path.Should().Be(@"C:\Logs\only");
    }

    [Test]
    public static void ServedPlatform_SectionAlsoKeyedByAPlatformTypeName_IgnoresThatEntry()
    {
        /* Arrange - a section stating both a label and the app's own name for the same platform, bound
           from this JSON alone so it states nothing else. */
        var appSettings = BindSettingsWithoutBaseFile(
            """
            {
              "Platform": "DrunkenKnitwits",
              "Logging": {
                "Platforms": {
                  "DK": { "Path": "C:\\Logs\\dk" },
                  "DrunkenKnitwits": { "Path": "C:\\Logs\\stale" }
                }
              }
            }
            """);

        // Act
        var logging = ServedPlatform.Of(appSettings, appSettings.Logging.Platforms);

        /* Assert - the binder converts a key through the labels alone and drops what it cannot convert, so
           the name contributes nothing. Do not make both spellings bind: the entry would then be decided by
           whichever key sorted last, not by which config layer stated it. */
        appSettings.Logging.Platforms.Should().ContainSingle();
        logging.Path.Should().Be(@"C:\Logs\dk");
    }

    [Test]
    public static void PlatformProvider_ReadsEveryPlatformsUrl()
    {
        /* Arrange - a deployment serves one platform but has to be able to name any of them: a Stripe
           webhook is actioned as the platform its payment was made on, whichever endpoint received it. */
        var appSettings = BindAppSettings(
            """
            {
              "Platform": "DrunkenKnitwits",
              "Platforms": {
                "DK": { "Url": "https://drunkenknitwits.example.com" },
                "GS": { "Url": "https://groupsquirrel.example.com" }
              }
            }
            """);

        // Act
        var provider = new PlatformProvider(MapPlatformProviderSettings(appSettings));

        // Assert
        provider.Platform.Should().Be(PlatformType.DrunkenKnitwits);
        provider.GetBaseUrl(PlatformType.Default).Should().Be("https://groupsquirrel.example.com");
        provider.GetBaseUrl(PlatformType.DrunkenKnitwits).Should().Be("https://drunkenknitwits.example.com");
    }

    [Test]
    public static void PlatformProvider_UrlNotStated_Throws()
    {
        // Arrange - the committed file states an empty URL, so an environment that adds none binds "".
        var appSettings = BindAppSettings("""{ "Platform": "Default" }""");
        var provider = new PlatformProvider(MapPlatformProviderSettings(appSettings));

        // Act
        var act = () => provider.GetBaseUrl(PlatformType.Default);

        // Assert - naming the platform, since the caller cannot see which one it asked about otherwise.
        act.Should().Throw<OdkServiceException>()
            .Which.Messages.Should().ContainSingle()
            .Which.Should().Contain(nameof(PlatformType.Default));
    }

    private static AppSettings BindSettingsWithoutBaseFile(string json)
        => new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)))
            .Build()
            .Get<AppSettings>()!;

    // The real appsettings.json, which the web project copies to this project's output, with an environment's
    // own file layered over it the way a deployment's is.
    private static AppSettings BindAppSettings(string environmentJson)
        => new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(environmentJson)))
            .Build()
            .Get<AppSettings>()!;

    // The registration itself, rather than a restatement of it: the fallback lives in the mapping, which is
    // the only place a config-bound value's absence can be coped with.
    private static PlatformProviderSettings MapPlatformProviderSettings(AppSettings appSettings)
    {
        var services = new ServiceCollection();
        services.ConfigureDependencies(appSettings);

        return (PlatformProviderSettings)services
            .Single(x => x.ServiceType == typeof(PlatformProviderSettings))
            .ImplementationInstance!;
    }
}
