using System.Diagnostics;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
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
    private readonly Stopwatch _stopwatch = new();

    protected abstract string PlatformBaseUrl { get; }

    protected ActivationTokenDataHelper ActivationTokenDataHelper
        => new ActivationTokenDataHelper(E2ESettings.ConnectionString);

    protected ChapterDataHelper ChapterDataHelper
        => new ChapterDataHelper(E2ESettings.ConnectionString);

    protected SentEmailDataHelper SentEmailDataHelper
        => new SentEmailDataHelper(E2ESettings.ConnectionString);

    public override BrowserNewContextOptions ContextOptions() => new() { BaseURL = PlatformBaseUrl };

    // Progress + per-test timing streamed live (dotnet test buffers normal test output; Progress does
    // not), so a long run shows which test is running now and how long each took. These run alongside
    // Playwright's own [SetUp]/[TearDown]; the timing brackets the whole test including provisioning.
    [SetUp]
    public void LogTestStarting()
    {
        _stopwatch.Restart();
        TestContext.Progress.WriteLine($"START        {Describe()}");
    }

    [TearDown]
    public void LogTestFinished()
    {
        _stopwatch.Stop();
        var status = TestContext.CurrentContext.Result.Outcome.Status;
        var marker = status == TestStatus.Passed ? "PASS" : status.ToString().ToUpperInvariant();
        TestContext.Progress.WriteLine($"{marker,-6} {_stopwatch.Elapsed.TotalSeconds,6:0.0}s {Describe()}");
    }

    private static string Describe()
    {
        var test = TestContext.CurrentContext.Test;
        var className = test.ClassName?.Split('.').LastOrDefault() ?? "?";
        return $"{className}.{test.Name}";
    }
}