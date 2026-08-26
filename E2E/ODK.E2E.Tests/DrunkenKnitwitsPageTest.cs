using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Tests.Config;

namespace ODK.E2E.Tests;

/// <summary>
/// Base for E2E fixtures targeting the <c>DrunkenKnitwits</c> platform. Adds the <c>DrunkenKnitwits</c>
/// category (filter with <c>--filter "TestCategory=DrunkenKnitwits"</c>) and points the browser at the
/// DrunkenKnitwits port. DrunkenKnitwits is chapter-scoped and has no self-service group creation, so
/// its fixtures cover different journeys (chapter join = sign-up, activate, log in) than Default.
/// </summary>
[Category("DrunkenKnitwits")]
public abstract class DrunkenKnitwitsPageTest : OdkPageTest
{
    protected override string PlatformBaseUrl => E2ESettings.DrunkenKnitwitsBaseUrl;

    /// <summary>
    /// This platform as the database stores it, for arranging rows on the platform whose pages the
    /// fixture drives.
    /// </summary>
    protected int PlatformTypeId => PlatformTypeIds.DrunkenKnitwits;
}
