using FluentAssertions;

namespace ODK.Web.Razor.Tests.Views;

/// <summary>
/// The web project's own .cshtml files, for the tests that scan the markup itself.
/// </summary>
internal static class ViewFiles
{
    private static readonly string[] ExcludedDirectories = ["bin", "lib", "node_modules", "obj"];

    /// <summary>Every .cshtml file under <paramref name="directory"/>, generated output aside.</summary>
    internal static IEnumerable<string> All(string directory)
    {
        foreach (var file in Directory.EnumerateFiles(directory, "*.cshtml"))
        {
            yield return file;
        }

        foreach (var child in Directory.EnumerateDirectories(directory))
        {
            if (!ExcludedDirectories.Contains(Path.GetFileName(child), StringComparer.OrdinalIgnoreCase))
            {
                foreach (var file in All(child))
                {
                    yield return file;
                }
            }
        }
    }

    internal static string ProjectDirectory()
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
}
