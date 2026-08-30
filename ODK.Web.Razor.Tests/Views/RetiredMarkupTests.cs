using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace ODK.Web.Razor.Tests.Views;

/// <summary>
/// Guards that the spacing and container idioms the section conventions replaced stay gone.
/// </summary>
/// <remarks>
/// A class removed from the SCSS is silent in the markup that still names it: the page renders, the build
/// passes, and the element simply gets no styling. So nothing stops one of the retired idioms coming back
/// except a scan of the markup itself. See the section conventions in CLAUDE.md for what replaced each.
/// </remarks>
[Parallelizable]
public static class RetiredMarkupTests
{
    /* Each entry is a class the section conventions retired, and what to use instead. Matched as a whole
       word inside a class attribute, so `mt-section` does not trip the `section--admin` rule. */
    private static readonly (string Class, string Replacement)[] RetiredClasses =
    [
        ("section", "a `.section` in the markup means the retired inert wrapper - render Components/_Section"),
        ("section--admin", "wrap the sections in a `.section-stack`"),
        ("section--main", "renamed to `.page-main`"),
        ("section--header", "renamed to `.page-header`"),
        ("section--footer", "renamed to `.page-footer`"),
        ("section--hero", "renamed to `.band--hero`"),
        ("section--dark", "renamed to `.band--dark`"),
        ("section--light", "renamed to `.band--light`"),
        ("section--grey", "renamed to `.band--grey`"),
        ("section--chapters", "renamed to `.band--chapters`"),
        ("sidebar--cards", "wrap the sidebar's panels in a `.section-stack`")
    ];

    // The component that emits `.section` - the one place the class is meant to appear.
    private static readonly string SectionComponent =
        Path.Combine("Views", "Shared", "Components", "_Section.cshtml");

    [Test]
    public static void Views_NoRetiredClassIsUsed()
    {
        // Arrange
        var projectDirectory = ViewFiles.ProjectDirectory();

        // Act
        var found = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        var scanned = 0;

        foreach (var file in ViewFiles.All(projectDirectory))
        {
            scanned++;
            var text = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(projectDirectory, file);

            foreach (var (retired, replacement) in RetiredClasses)
            {
                if (retired == "section" && relativePath == SectionComponent)
                {
                    continue;
                }

                /* Only inside a class attribute: the names also appear in the SCSS-facing comments and, for
                   "section", in every `data-odk-component="_Section"` and `Components/_Section` path. */
                var pattern = new Regex(
                    $@"class\s*=\s*""[^""]*(?<![\w-]){Regex.Escape(retired)}(?![\w-])[^""]*""");

                if (pattern.IsMatch(text))
                {
                    found.Add($"{relativePath} uses .{retired} - {replacement}");
                }
            }
        }

        // Assert - on the scan first, so a walk that stops finding views fails here rather than passing
        // vacuously.
        scanned.Should().BeGreaterThan(100, "the web project has hundreds of views, so the scan must find them");

        found.Should().BeEmpty(
            "these views use a class the section conventions retired:{0}{1}",
            Environment.NewLine,
            string.Join(Environment.NewLine, found));
    }
}
