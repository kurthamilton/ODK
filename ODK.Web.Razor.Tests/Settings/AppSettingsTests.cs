using System.Collections;
using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
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
        // Arrange
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        // Act
        var settings = config.Get<AppSettings>();

        // Assert
        settings.Should().NotBeNull();

        var nullPaths = new List<string>();

        // Created here rather than shared: NullabilityInfoContext is not thread-safe, and the class is parallelizable.
        AddNullPaths(settings!, nameof(AppSettings), nullPaths, new NullabilityInfoContext());
        nullPaths.Should().BeEmpty(
            "every non-nullable setting must be stated in config, but these bound to null: {0}",
            string.Join(", ", nullPaths));
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
