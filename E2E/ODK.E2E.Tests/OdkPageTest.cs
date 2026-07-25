using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Tests.Config;

namespace ODK.E2E.Tests;

/// <summary>
/// Base for all E2E fixtures. Carries the <c>E2E</c> category and marks the suite <see cref="ExplicitAttribute"/>
/// (needs a live app + browsers). Derive from a platform base (<see cref="DefaultPageTest"/> /
/// <see cref="DrunkenKnitwitsPageTest"/>) rather than this directly: the platform base sets the browser
/// context's <see cref="BrowserNewContextOptions.BaseURL"/>, so page objects navigate with relative
/// paths and each fixture hits the correct platform port.
/// </summary>
[Category("E2E")]
[Explicit("Requires a running instance, its database, and installed Playwright browsers.")]
public abstract class OdkPageTest : PageTest
{
    protected abstract string PlatformBaseUrl { get; }

    protected ActivationTokenDataHelper ActivationTokenDataHelper
        => new ActivationTokenDataHelper(E2ESettings.ConnectionString);

    protected ChapterDataHelper ChapterDataHelper
        => new ChapterDataHelper(E2ESettings.ConnectionString);

    protected SentEmailDataHelper SentEmailDataHelper
        => new SentEmailDataHelper(E2ESettings.ConnectionString);

    public override BrowserNewContextOptions ContextOptions() => new() { BaseURL = PlatformBaseUrl };
}