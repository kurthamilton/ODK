using System.Collections.Generic;
using ODK.Core.Platforms;
using ODK.Services.Platforms;

namespace ODK.Services.Tests.Helpers;

/// <summary>
/// The real provider over test names, rather than a double: it does nothing but read a configured name, so
/// a fake would only restate the lookup, and a test asserting on a name should exercise the resolution it
/// depends on.
/// </summary>
internal static class TestPlatformProvider
{
    internal const string DefaultBaseUrl = "https://default.example.com";

    internal const string DefaultName = "Default platform";

    internal const string DrunkenKnitwitsBaseUrl = "https://drunkenknitwits.example.com";

    internal const string DrunkenKnitwitsName = "Drunken Knitwits platform";

    /// <param name="defaultName">
    /// The default platform's name, for a test that turns on what the name itself contains.
    /// </param>
    /// <param name="platform">The platform the deployment under test serves.</param>
    internal static IPlatformProvider Create(
        string? defaultName = null,
        PlatformType platform = PlatformType.Default) => new PlatformProvider(
        new PlatformProviderSettings
        {
            BaseUrls = new Dictionary<PlatformType, string>
            {
                { PlatformType.Default, DefaultBaseUrl },
                { PlatformType.DrunkenKnitwits, DrunkenKnitwitsBaseUrl }
            },
            Names = new Dictionary<PlatformType, string>
            {
                { PlatformType.Default, defaultName ?? DefaultName },
                { PlatformType.DrunkenKnitwits, DrunkenKnitwitsName }
            },
            Platform = platform
        });
}
