using FluentAssertions;
using ODK.Services.Integrations.Html;

namespace ODK.Services.Integrations.Tests.Html;

[Parallelizable]
public static class HtmlTextExtractorTests
{
    [Test]
    public static void ToPlainText_AdjacentBlocks_SeparatesThem()
    {
        // Arrange - TextContent concatenates text nodes with nothing between them, so this is "OneTwo"
        // without the separators the extractor inserts.
        var html = "<p>One</p><p>Two</p>";

        // Act
        var result = new HtmlTextExtractor().ToPlainText(html);

        // Assert
        result.Should().Be("One Two");
    }

    [TestCase("<ul><li>A</li><li>B</li></ul>", "A B")]
    [TestCase("<ol><li>A</li><li>B</li></ol>", "A B")]
    [TestCase("<p>Line<br/>break</p>", "Line break")]
    [TestCase("<h2>Heading</h2><p>Body</p>", "Heading Body")]
    [TestCase("<div>One</div><div>Two</div>", "One Two")]
    [TestCase("<table><tr><td>R1C1</td><td>R1C2</td></tr><tr><td>R2C1</td></tr></table>", "R1C1 R1C2 R2C1")]
    public static void ToPlainText_BlockBoundary_BecomesASpace(string html, string expected)
    {
        // Act
        var result = new HtmlTextExtractor().ToPlainText(html);

        // Assert
        result.Should().Be(expected);
    }

    [TestCase("<div>Un<span>split</span>table</div>", "Unsplittable")]
    [TestCase("<p>Bold <strong>word</strong> inline</p>", "Bold word inline")]
    [TestCase("<p>An <a href=\"/x\">anchor</a> here</p>", "An anchor here")]
    public static void ToPlainText_InlineElement_DoesNotSplitTheWord(string html, string expected)
    {
        // Arrange - the opposite case to the block boundary: an inline element sits inside a word as
        // often as between two, so inserting a separator around one would break the text.

        // Act
        var result = new HtmlTextExtractor().ToPlainText(html);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public static void ToPlainText_Entities_AreDecoded()
    {
        // Arrange - the reason this is parsed rather than pattern-matched: a regex over tags leaves every
        // one of these as its source text.
        var html = "<p>Hello&nbsp;&amp;&nbsp;welcome &mdash; we&rsquo;re here</p>";

        // Act
        var result = new HtmlTextExtractor().ToPlainText(html);

        // Assert - the non-breaking spaces &nbsp; decodes to are normalised to plain ones.
        result.Should().Be("Hello & welcome — we’re here");
    }

    [TestCase("script", "alert('x')")]
    [TestCase("style", "p{color:red}")]
    public static void ToPlainText_ScriptOrStyle_IsRemovedWithItsContents(string tag, string contents)
    {
        // Arrange - the text inside these is code, and TextContent returns it like any other text node.
        var html = $"<p>Text</p><{tag}>{contents}</{tag}>";

        // Act
        var result = new HtmlTextExtractor().ToPlainText(html);

        // Assert
        result.Should().Be("Text");
    }

    [Test]
    public static void ToPlainText_MalformedMarkup_StillReadsTheText()
    {
        // Arrange - stored values predate the validator, so unclosed tags have to survive this.
        var html = "<p>Unclosed<p>Second<ul><li>Item";

        // Act
        var result = new HtmlTextExtractor().ToPlainText(html);

        // Assert
        result.Should().Be("Unclosed Second Item");
    }

    [TestCase("Plain text with no tags", "Plain text with no tags")]
    [TestCase("<p>   </p><p>Only this</p>", "Only this")]
    [TestCase("<p>  Padded  </p>", "Padded")]
    [TestCase("<p>Multiple\n\nlines</p>", "Multiple lines")]
    public static void ToPlainText_Whitespace_IsCollapsedAndTrimmed(string html, string expected)
    {
        // Act
        var result = new HtmlTextExtractor().ToPlainText(html);

        // Assert
        result.Should().Be(expected);
    }

    [TestCase(null)]
    [TestCase("")]
    public static void ToPlainText_NoContent_ReturnsEmpty(string? html)
    {
        // Act
        var result = new HtmlTextExtractor().ToPlainText(html);

        // Assert
        result.Should().BeEmpty();
    }
}
