using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace ODK.Web.Razor.Tests.Assets;

/// <summary>
/// Guards that every client library asset the app asks for is present in wwwroot/lib.
/// </summary>
/// <remarks>
/// wwwroot/lib is not committed. build/copy-client-libs.mjs copies a listed subset of each npm package into
/// it, so what gets served is a decision taken in that one file - and nothing else notices when the subset is
/// missing something. An absent script, stylesheet or webfont compiles, publishes and deploys perfectly well,
/// then 404s in the browser. These tests read the references back out of the source and out of the copied
/// stylesheets and check every one, which is what makes trimming a package a reviewable change rather than a
/// gamble.
/// </remarks>
[Parallelizable]
public static class ClientLibraryAssetTests
{
    /* Only a path carrying one of these extensions is checked. A Sass @import has none and resolves through
       Sass's own partial rules (lib/bootstrap/scss/variables is _variables.scss), and the SCSS build already
       fails on a missing one, so those need nothing here. */
    private static readonly string[] AssetExtensions =
        [".css", ".js", ".json", ".map", ".mjs", ".svg", ".woff", ".woff2"];

    // "lib" is wwwroot/lib, the output being verified - reading it back as a source of references would make
    // the test agree with itself.
    private static readonly string[] ExcludedDirectories = ["bin", "lib", "node_modules", "obj"];

    // The lookbehind stops the match starting inside a longer word, so "tslib/..." is not a lib/ reference.
    private static readonly Regex LibraryReference = new(
        @"(?<![\w.-])lib/(?<path>[\w.\-/]+\.[a-zA-Z0-9]+)", RegexOptions.Compiled);

    private static readonly string[] SourceExtensions = [".cs", ".cshtml", ".js"];

    private static readonly Regex StylesheetUrl = new(
        @"url\(\s*(?<quote>[""']?)(?<url>[^""')]+)\k<quote>\s*\)", RegexOptions.Compiled);

    [Test]
    public static void ClientLibraries_EveryReferencedAssetExists()
    {
        // Arrange
        var projectDirectory = ProjectDirectory();
        var webRoot = Path.Combine(projectDirectory, "wwwroot");

        // Act
        var references = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in SourceFiles(projectDirectory))
        {
            foreach (Match match in LibraryReference.Matches(File.ReadAllText(file)))
            {
                var path = match.Groups["path"].Value;
                if (AssetExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
                {
                    references.Add($"lib/{path}");
                }
            }
        }

        // Assert - on the scan first, so a regex that stops matching fails here rather than passing vacuously.
        references.Should().NotBeEmpty("the app references client library assets, so the scan must find some");

        var missing = references
            .Where(x => !File.Exists(Path.Combine(webRoot, x.Replace('/', Path.DirectorySeparatorChar))))
            .ToArray();
        missing.Should().BeEmpty(
            "every lib/ path the app references must be copied by build/copy-client-libs.mjs, but these are "
            + "absent from wwwroot: {0}",
            string.Join(", ", missing));
    }

    [Test]
    public static void ClientLibraries_EveryStylesheetUrlResolves()
    {
        // Arrange - the assets a stylesheet pulls in are named nowhere a build or a source scan can see them,
        // which is how a trimmed package loses its webfonts without anything complaining.
        var webRoot = Path.Combine(ProjectDirectory(), "wwwroot");
        var libDirectory = Path.Combine(webRoot, "lib");

        Directory.Exists(libDirectory).Should().BeTrue(
            "wwwroot/lib is written by the build - run a build of ODK.Web.Razor before this test");

        // Act
        var missing = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var stylesheet in Directory.EnumerateFiles(libDirectory, "*.css", SearchOption.AllDirectories))
        {
            foreach (Match match in StylesheetUrl.Matches(File.ReadAllText(stylesheet)))
            {
                var url = WithoutQuery(match.Groups["url"].Value.Trim());

                if (url.Length == 0 || url.StartsWith("data:") || Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    continue;
                }

                var root = url.StartsWith('/') ? webRoot : Path.GetDirectoryName(stylesheet)!;
                var target = Path.GetFullPath(Path.Combine(root, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));

                if (!File.Exists(target))
                {
                    missing.Add($"{Path.GetRelativePath(webRoot, stylesheet)} -> {url}");
                }
            }
        }

        // Assert
        missing.Should().BeEmpty(
            "every asset a copied stylesheet references must be copied alongside it, but these are absent: {0}",
            string.Join(", ", missing));
    }

    private static string ProjectDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "odk.slnx")))
        {
            directory = directory.Parent;
        }

        directory.Should().NotBeNull(
            "these tests read the web project's own files, so they must run from inside the repository - no "
            + "odk.slnx was found above {0}",
            AppContext.BaseDirectory);

        return Path.Combine(directory.FullName, "ODK.Web.Razor");
    }

    private static IEnumerable<string> SourceFiles(string directory)
    {
        foreach (var file in Directory.EnumerateFiles(directory))
        {
            if (SourceExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
            {
                yield return file;
            }
        }

        foreach (var child in Directory.EnumerateDirectories(directory))
        {
            if (ExcludedDirectories.Contains(Path.GetFileName(child), StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            foreach (var file in SourceFiles(child))
            {
                yield return file;
            }
        }
    }

    private static string WithoutQuery(string url)
    {
        var end = url.IndexOfAny(['?', '#']);
        return end < 0 ? url : url[..end];
    }
}
