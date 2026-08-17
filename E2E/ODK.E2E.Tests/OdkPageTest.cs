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

    // Tracing every test so a failure can be replayed costs time and memory on a suite this size, so it
    // can be turned off for a run where speed matters more than diagnosing a failure.
    private static readonly bool ArtifactsEnabled =
        string.IsNullOrEmpty(Environment.GetEnvironmentVariable("E2E_NO_ARTIFACTS"));

    private static int _completed;

    private readonly Stopwatch _stopwatch = new();

    protected abstract string PlatformBaseUrl { get; }

    protected ActivationTokenDataHelper ActivationTokenDataHelper
        => new ActivationTokenDataHelper(E2ESettings.ConnectionString);

    protected ChapterDataHelper ChapterDataHelper
        => new ChapterDataHelper(E2ESettings.ConnectionString);

    protected MemberChapterInviteDataHelper MemberChapterInviteDataHelper
        => new MemberChapterInviteDataHelper(E2ESettings.ConnectionString);

    protected SentEmailDataHelper SentEmailDataHelper
        => new SentEmailDataHelper(E2ESettings.ConnectionString);

    protected SiteQuestionDataHelper SiteQuestionDataHelper
        => new SiteQuestionDataHelper(E2ESettings.ConnectionString);

    // Pin the locale to the default so rendered date/number formatting is deterministic regardless of the
    // host's locale (the app applies the request locale to rendering). Parsing of posted values is
    // unaffected - the app binds under a fixed default culture. Locale-specific tests set their own Locale.
    public override BrowserNewContextOptions ContextOptions() =>
        new() { BaseURL = PlatformBaseUrl, Locale = "en-GB" };

    // Progress + per-test timing streamed live (dotnet test buffers normal test output; Progress does
    // not), so a long run shows which test is running now, how long each took, and a running count of
    // completed tests. PASS/FAIL is colour-highlighted. Fixtures run in parallel, so lines from different
    // fixtures interleave - each names its test. These run alongside Playwright's own [SetUp]/[TearDown];
    // the timing brackets the whole test including provisioning.
    [SetUp]
    public async Task LogTestStarting()
    {
        _stopwatch.Restart();
        TestContext.Progress.WriteLine(Paint($"  ··   {Describe()}", Dim));

        await StartTracing();
    }

    // Runs before Playwright's own [TearDown] - NUnit tears down derived-first - so Context and Page are
    // still open here, which is what lets the trace be stopped and the screenshot taken.
    [TearDown]
    public async Task LogTestFinished()
    {
        _stopwatch.Stop();

        var failed = TestContext.CurrentContext.Result.Outcome.Status
            is not TestStatus.Passed and not TestStatus.Skipped;

        var artifacts = await CaptureArtifacts(failed);

        var (label, colour) = TestContext.CurrentContext.Result.Outcome.Status switch
        {
            TestStatus.Passed => ("PASS", Green),
            TestStatus.Skipped => ("SKIP", Yellow),
            _ => ("FAIL", RedBold)
        };

        var count = Interlocked.Increment(ref _completed);
        TestContext.Progress.WriteLine(
            $"{Paint(label, colour)} {_stopwatch.Elapsed.TotalSeconds,6:0.0}s  {Paint($"[{count}]", Cyan)}  {Describe()}");

        foreach (var artifact in artifacts)
        {
            TestContext.Progress.WriteLine(Paint($"       {artifact}", Dim));
        }
    }

    private static string Ansi(string code) => $"{(char)27}[{code}m";

    /// <summary>
    /// Where failure artifacts are written: the project's <c>TestResults</c>, alongside the html report,
    /// so a run's output is in one place and script.e2e.bat can name the path. Found by walking up from
    /// the assembly (which lives in bin/Debug/net10.0) to the directory holding the csproj - WorkDirectory
    /// is the bin folder under `dotnet test`, which would scatter the artifacts away from the report.
    /// </summary>
    private static string ArtifactDirectory()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory != null && directory.GetFiles("*.csproj").Length == 0)
        {
            directory = directory.Parent;
        }

        var root = directory?.FullName ?? TestContext.CurrentContext.WorkDirectory;
        var artifacts = Path.Combine(root, "TestResults", "artifacts");
        Directory.CreateDirectory(artifacts);
        return artifacts;
    }

    /// <summary>
    /// A test name reduced to something safe for a filename - <c>[TestCase]</c> arguments bring quotes,
    /// commas and parentheses with them.
    /// </summary>
    private static string ArtifactName()
    {
        var name = Describe();
        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(invalid, '-');
        }

        return name.Replace(' ', '-').Replace(",", string.Empty);
    }

    private static string Describe()
    {
        var test = TestContext.CurrentContext.Test;
        var className = test.ClassName?.Split('.').LastOrDefault() ?? "?";
        return $"{className}.{test.Name}";
    }

    private static string Paint(string text, string colour) => Colour ? $"{colour}{text}{Reset}" : text;

    /// <summary>
    /// Stops tracing, keeping the trace and a screenshot only when the test failed. Tracing has to be
    /// started for every test - it cannot be turned on retrospectively once something has gone wrong - so
    /// a passing test stops it without a path, which discards it.
    ///
    /// Never throws: a failure to capture an artifact must not replace the real test failure with a
    /// confusing one, and must not turn a passing test red.
    /// </summary>
    private async Task<IReadOnlyList<string>> CaptureArtifacts(bool failed)
    {
        if (!ArtifactsEnabled)
        {
            return [];
        }

        var artifacts = new List<string>();

        try
        {
            var path = failed ? Path.Combine(ArtifactDirectory(), $"{ArtifactName()}.zip") : null;
            await Context.Tracing.StopAsync(new() { Path = path });
            if (path != null)
            {
                artifacts.Add($"trace: {path}");
            }
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine(Paint($"       trace not saved: {ex.Message}", Yellow));
        }

        if (!failed)
        {
            return artifacts;
        }

        try
        {
            var path = Path.Combine(ArtifactDirectory(), $"{ArtifactName()}.png");
            await Page.ScreenshotAsync(new() { Path = path, FullPage = true });
            artifacts.Add($"screenshot: {path}");
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine(Paint($"       screenshot not saved: {ex.Message}", Yellow));
        }

        return artifacts;
    }

    /// <summary>
    /// Records actions, DOM snapshots and sources for the whole test. Only kept when the test fails (see
    /// <see cref="CaptureArtifacts"/>), but it has to run for every test because a trace cannot be
    /// started after the fact - set <c>E2E_NO_ARTIFACTS</c> to skip it when the overhead isn't worth it.
    /// </summary>
    private async Task StartTracing()
    {
        if (!ArtifactsEnabled)
        {
            return;
        }

        try
        {
            await Context.Tracing.StartAsync(new()
            {
                Screenshots = true,
                Snapshots = true,
                Sources = true,
                Title = Describe()
            });
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine(Paint($"       tracing not started: {ex.Message}", Yellow));
        }
    }
}
