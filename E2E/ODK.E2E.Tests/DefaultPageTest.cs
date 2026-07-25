using NUnit.Framework;
using ODK.E2E.Tests.Config;

namespace ODK.E2E.Tests;

/// <summary>
/// Base for E2E fixtures targeting the <c>Default</c> platform. Adds the <c>Default</c> category
/// (filter with <c>--filter "TestCategory=Default"</c>) and points the browser at the Default port.
/// </summary>
[Category("Default")]
public abstract class DefaultPageTest : OdkPageTest
{
    protected override string PlatformBaseUrl => E2ESettings.DefaultBaseUrl;
}