using System.Collections;
using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ODK.Core.Platforms;
using ODK.Infrastructure;
using ODK.Infrastructure.Settings;

namespace ODK.Web.Razor.Tests.Settings;

/// <summary>
/// Guards that appsettings.json states every value the settings types declare.
/// </summary>
/// <remarks>
/// Declaring a property <c>required</c> cannot do this. The configuration binder constructs settings
/// reflectively rather than through an object initialiser, so a key missing from config binds to null however
/// the property is declared, and the failure lands wherever the value is first dereferenced - at startup in
/// <c>DependencyRegistrar</c> if it is lucky, and on the request path if it is not. This walks the bound graph
/// instead and names every null, so a key removed from config fails here rather than in production.
/// </remarks>
[Parallelizable]
public static class AppSettingsTests
{
    [Test]
    public static void AppSettings_EveryReferenceValueIsStatedInConfig()
    {
        // Act
        var settings = BindAppSettings();

        // Assert
        settings.Should().NotBeNull();

        var nullPaths = new List<string>();

        // Created here rather than shared: NullabilityInfoContext is not thread-safe, and the class is parallelizable.
        AddNullPaths(settings!, nameof(AppSettings), nullPaths, new NullabilityInfoContext());
        nullPaths.Should().BeEmpty(
            "every non-nullable setting must be stated in config, but these bound to null: {0}",
            string.Join(", ", nullPaths));
    }

    /* The committed file is the template every environment layers over, so a value that differs per
       deployment is stated empty rather than given a working default: a default is a value nobody reviewing
       the config can see, and it is what a deployment that forgot to state its own would silently run as. */
    [Test]
    public static void AppSettings_PerDeploymentValuesAreLeftUnstated()
    {
        // Act
        var settings = BindAppSettings();

        // Assert
        settings.Platform.Should().Be(
            PlatformType.None,
            "the platform a deployment serves is stated by that deployment, not by the committed file");
        settings.Platforms.Values.Select(x => x.Url).Should().AllSatisfy(
            url => url.Should().BeEmpty("a platform's URL differs per environment"));
        settings.Logging.Platforms.Values.Select(x => x.Path).Should().AllSatisfy(
            path => path.Should().BeEmpty("a platform's log directory differs per environment"));
        settings.BetterStack.Platforms.Values.Should().AllSatisfy(source =>
        {
            source.SourceToken.Should().BeEmpty("a platform's log source is a secret, so it is never committed");
            source.IngestingHost.Should().BeEmpty("a platform's ingesting host belongs to its source");
        });
    }

    [Test]
    public static void ConfigureDependencies_DoesNotRegisterAppSettings()
    {
        /* Arrange - every consumer takes a mapped settings type declaring the values it uses. Nothing injects
           AppSettings itself, and nothing should be able to: injecting an unregistered type fails when the
           request is served rather than when the app builds, so a test is the only thing that catches it. */
        var services = new ServiceCollection();

        // Act
        services.ConfigureDependencies(BindAppSettings());

        // Assert - on the one fact, not the collection, which is several hundred registrations long.
        var registered = services.Any(x => x.ServiceType == typeof(AppSettings));
        registered.Should().BeFalse("AppSettings must not be resolvable - consumers take a mapped settings type");
    }

    private static void AddChildNullPaths(
        object value, string path, List<string> paths, NullabilityInfoContext nullability)
    {
        switch (value)
        {
            case string:
                return;

            case IDictionary dictionary:
                foreach (var key in dictionary.Keys)
                {
                    AddItemNullPaths(dictionary[key], $"{path}:{key}", paths, nullability);
                }

                return;

            case IEnumerable enumerable:
                var index = 0;
                foreach (var item in enumerable)
                {
                    AddItemNullPaths(item, $"{path}[{index++}]", paths, nullability);
                }

                return;

            default:
                // Only the settings types are walked, not framework types they happen to expose.
                if (value.GetType().Namespace == typeof(AppSettings).Namespace)
                {
                    AddNullPaths(value, path, paths, nullability);
                }

                return;
        }
    }

    // The real appsettings.json, which the web project copies to this project's output.
    private static AppSettings BindAppSettings()
        => new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build()
            .Get<AppSettings>()!;

    private static void AddItemNullPaths(
        object? item, string path, List<string> paths, NullabilityInfoContext nullability)
    {
        if (item == null)
        {
            paths.Add(path);
            return;
        }

        AddChildNullPaths(item, path, paths, nullability);
    }

    /* Two kinds of property are deliberately skipped.

       Value types, because an absent int binds to 0 and an absent bool to false. That is a wrong value rather
       than a missing one, and nothing about the bound object distinguishes it from a 0 config stated on purpose.

       Properties declared nullable, because there the author has said null is a value the code handles -
       Emails:DebugEmailAddress is null in config precisely to mean "do not redirect". Only a non-nullable
       reference arriving null is a broken promise, and that is what this looks for. */
    private static void AddNullPaths(
        object settings, string path, List<string> paths, NullabilityInfoContext nullability)
    {
        foreach (var property in settings.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.PropertyType.IsValueType ||
                nullability.Create(property).ReadState == NullabilityState.Nullable)
            {
                continue;
            }

            var propertyPath = $"{path}:{property.Name}";
            var value = property.GetValue(settings);

            if (value == null)
            {
                paths.Add(propertyPath);
                continue;
            }

            AddChildNullPaths(value, propertyPath, paths, nullability);
        }
    }
}
