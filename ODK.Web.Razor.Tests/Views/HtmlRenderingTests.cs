using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace ODK.Web.Razor.Tests.Views;

/// <summary>
/// Guards the Html suffix convention where it is rendered: <c>Html.Raw</c> renders a value whose name ends
/// in Html and nothing else, and a value whose name ends in Html is never rendered bare.
/// </summary>
/// <remarks>
/// Both mistakes compile and render. HTML through the encoder shows its own tags on the page; text through
/// <c>Html.Raw</c> is an injection hole. A string carries no trace of which it holds, so the name is the
/// only thing telling the two apart, and a scan of the markup is the only thing holding the name to it.
/// See the Html suffix convention in CLAUDE.md.
/// </remarks>
[Parallelizable]
public static class HtmlRenderingTests
{
    /* A value whose name ends in Html that a view deliberately renders as text. Rare by construction: it
       means showing markup as its own source, which is a thing to display rather than a thing to render. */
    private static readonly (string File, string Expression)[] AllowedBareRenders =
    [
        // Shown as source in a <pre>, so a group can compare the default wording against its own.
        (Path.Combine("Views", "Shared", "Admin", "Chapter", "_ChapterAdminEmailForm.cshtml"),
            "@Model.InheritedContentHtml")
    ];

    private static readonly Regex Identifier = new(@"[A-Za-z_]\w*");

    /* A Razor implicit expression - @, an optional opening bracket, then a member chain. Anything the
       chain calls ends the match, so `@Html.Raw(x)` reads as `@Html.Raw` and is judged on `Raw`. */
    private static readonly Regex ImplicitExpression = new(
        @"@\(?\s*(?:await\s+)?[A-Za-z_]\w*(?:\??\.[A-Za-z_]\w*|\[[^\]]*\])*");

    [Test]
    public static void Views_HtmlRawRendersOnlyHtmlSuffixedValues()
    {
        // Arrange
        var projectDirectory = ViewFiles.ProjectDirectory();

        // Act
        var found = new SortedSet<string>(StringComparer.Ordinal);
        var scanned = 0;

        foreach (var file in ViewFiles.All(projectDirectory))
        {
            scanned++;
            var text = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(projectDirectory, file);

            foreach (var (_, _, argument) in HtmlRawArguments(text))
            {
                if (!RendersHtml(argument))
                {
                    found.Add($"{relativePath} renders Html.Raw({argument.Trim()})");
                }
            }
        }

        // Assert - on the scan first, so a walk that stops finding views fails here rather than passing
        // vacuously.
        scanned.Should().BeGreaterThan(100, "the web project has hundreds of views, so the scan must find them");

        found.Should().BeEmpty(
            "Html.Raw renders a value whose name ends in Html - a value that does not hold HTML has to be "
            + "rendered through the encoder, and one that does has to say so in its name:{0}{1}",
            Environment.NewLine,
            string.Join(Environment.NewLine, found));
    }

    [Test]
    public static void Views_HtmlSuffixedValuesAreNotRenderedBare()
    {
        // Arrange
        var projectDirectory = ViewFiles.ProjectDirectory();

        // Act
        var found = new SortedSet<string>(StringComparer.Ordinal);
        var scanned = 0;

        foreach (var file in ViewFiles.All(projectDirectory))
        {
            scanned++;
            var text = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(projectDirectory, file);
            var rawArguments = HtmlRawArguments(text).ToArray();

            foreach (Match match in ImplicitExpression.Matches(text))
            {
                if (!RendersHtml(match.Value))
                {
                    continue;
                }

                // The argument of an Html.Raw is the one place the value is meant to be rendered.
                if (rawArguments.Any(x => match.Index > x.Start && match.Index < x.End))
                {
                    continue;
                }

                if (AllowedBareRenders.Contains((relativePath, match.Value)))
                {
                    continue;
                }

                found.Add($"{relativePath} renders {match.Value} without Html.Raw");
            }
        }

        // Assert
        scanned.Should().BeGreaterThan(100, "the web project has hundreds of views, so the scan must find them");

        found.Should().BeEmpty(
            "a value whose name ends in Html holds markup, so rendering it bare prints its tags on the page "
            + "- render it through Html.Raw, or drop the suffix if it is not HTML after all:{0}{1}",
            Environment.NewLine,
            string.Join(Environment.NewLine, found));
    }

    /// <summary>
    /// Every <c>Html.Raw(...)</c> in <paramref name="text"/>, with the span it occupies and the expression
    /// it renders. Bracket-matched rather than matched by regex, since an argument can carry brackets of
    /// its own.
    /// </summary>
    private static IEnumerable<(int Start, int End, string Argument)> HtmlRawArguments(string text)
    {
        foreach (Match call in Regex.Matches(text, @"Html\.Raw\("))
        {
            var index = call.Index + call.Length;
            var depth = 1;

            while (index < text.Length && depth > 0)
            {
                depth += text[index] switch
                {
                    '(' => 1,
                    ')' => -1,
                    _ => 0
                };

                index++;
            }

            yield return (call.Index, index, text[(call.Index + call.Length)..(index - 1)]);
        }
    }

    /// <summary>
    /// Whether an expression renders a value the convention calls HTML: the last name in it carries the
    /// suffix. <c>Html</c> on its own is the Razor helper rather than a value, so it does not count.
    /// </summary>
    private static bool RendersHtml(string expression)
    {
        var names = Identifier.Matches(expression);

        return names.Count > 0
            && names[^1].Value != "Html"
            && names[^1].Value.EndsWith("Html", StringComparison.Ordinal);
    }
}
