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
    private static readonly string Cyan = Ansi("36");
    private static readonly string Dim = Ansi("90");
    private static readonly string Green = Ansi("32");
    private static readonly string RedBold = Ansi("1;31");
    private static readonly string Reset = Ansi("0");
    private static readonly string Yellow = Ansi("33");

    // ANSI colour, only when writing to a real terminal. Under `dotnet test` the output is captured by
    // the VSTest host (Console.IsOutputRedirected is true), which re-encodes control characters as literal
    // text (e.g. [32m) instead of rendering them - so colour is disabled there and the PASS/FAIL/SKIP
    // tags print as clean plain text. Also honour NO_COLOR (https://no-color.org) as an explicit opt-out.
    private static readonly bool Colour =
        string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NO_COLOR"))
        && !Console.IsOutputRedirected;

    private static int _completed;

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
    // not), so a long run shows which test is running now, how long each took, and a running count of
    // completed tests. PASS/FAIL is colour-highlighted. Fixtures run in parallel, so lines from different
    // fixtures interleave - each names its test. These run alongside Playwright's own [SetUp]/[TearDown];
    // the timing brackets the whole test including provisioning.
    [SetUp]
    public void LogTestStarting()
    {
        _stopwatch.Restart();
        TestContext.Progress.WriteLine(Paint($"  ··   {Describe()}", Dim));
    }

    [TearDown]
    public void LogTestFinished()
    {
        _stopwatch.Stop();

        var (label, colour) = TestContext.CurrentContext.Result.Outcome.Status switch
        {
            TestStatus.Passed => ("PASS", Green),
            TestStatus.Skipped => ("SKIP", Yellow),
            _ => ("FAIL", RedBold)
        };

        var count = Interlocked.Increment(ref _completed);
        TestContext.Progress.WriteLine(
            $"{Paint(label, colour)} {_stopwatch.Elapsed.TotalSeconds,6:0.0}s  {Paint($"[{count}]", Cyan)}  {Describe()}");
    }

    private static string Ansi(string code) => $"{(char)27}[{code}m";

    private static string Describe()
    {
        var test = TestContext.CurrentContext.Test;
        var className = test.ClassName?.Split('.').LastOrDefault() ?? "?";
        return $"{className}.{test.Name}";
    }

    private static string Paint(string text, string colour) => Colour ? $"{colour}{text}{Reset}" : text;
}
