using NUnit.Framework;
using ODK.E2E.Data;
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

    /// <summary>
    /// This platform as the database stores it, for arranging rows on the platform whose pages the
    /// fixture drives.
    /// </summary>
    protected int PlatformTypeId => PlatformTypeIds.Default;
}