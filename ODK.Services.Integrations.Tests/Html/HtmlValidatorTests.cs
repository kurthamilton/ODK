using FluentAssertions;
using ODK.Services.Html;
using ODK.Services.Integrations.Html;

namespace ODK.Services.Integrations.Tests.Html;

[Parallelizable]
public static class HtmlValidatorTests
{
    private static readonly HtmlValidatorOptions Default = new() { AllowLinks = true };

    private static readonly HtmlValidatorOptions WellFormed =
        new() { AllowLinks = true, RequireWellFormed = true };

    [Test]
    public static void Validate_EditorMarkup_Passes()
    {
        // Arrange - what the TinyMCE toolbar produces has to be accepted, or admins cannot save at all.
        var html = "<p><strong>Bold</strong> and <em>italic</em></p><ul><li>One</li></ul>" +
            "<a href=\"https://example.com\" title=\"Example\">Link</a>" +
            "<table><tbody><tr><td colspan=\"2\">Cell</td></tr></tbody></table>";

        // Act
        var result = new HtmlValidator().Validate(html, Default);

        // Assert
        result.Success.Should().BeTrue(result.Message);
    }

    /* Was Sanitize_EncodesBlacklistedTags / _EncodesSelfClosingBlacklistedTags, which asserted these
       came back HTML-encoded so the content stayed on the page as inert text. Nothing is rewritten
       now - the save is rejected and the admin is told what to remove. Same five tags, new outcome. */
    [TestCase("embed")]
    [TestCase("form")]
    [TestCase("iframe")]
    [TestCase("object")]
    [TestCase("script")]
    public static void Validate_PreviouslyBlacklistedTag_Fails(string tag)
    {
        // Act
        var result = new HtmlValidator().Validate($"<p>Keep</p><{tag}>Content</{tag}>", Default);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain(tag);
    }

    [TestCase("embed")]
    [TestCase("form")]
    [TestCase("iframe")]
    [TestCase("object")]
    [TestCase("script")]
    public static void Validate_PreviouslyBlacklistedSelfClosingTag_Fails(string tag)
    {
        // Act
        var result = new HtmlValidator().Validate($"<{tag}/>", Default);

        // Assert
        result.Success.Should().BeFalse();
    }

    [TestCase("<p onmouseover=\"alert(1)\">Hi</p>",
        Description = "The old deny-list named only onload, onclick and onerror, so this got through.")]
    [TestCase("<style>body { color: red }</style>",
        Description = "style was on no list at all, so it was accepted.")]
    [TestCase("<svg><desc>x</desc></svg>",
        Description = "svg was on no list at all, so it was accepted.")]
    [TestCase("<base href=\"https://evil.example\">",
        Description = "base was on no list at all, so it was accepted.")]
    [TestCase("<p style=\"color: red\">Hi</p>",
        Description = "Inline styles never survive the editor, so accepting them only admitted pasted markup.")]
    [TestCase("<img/**/onmouseover=alert(1) src=x>",
        Description = "Spaced to dodge a regex expecting name=\"value\" - the parser sees it regardless.")]
    public static void Validate_ConstructTheDenyListMissed_Fails(string html)
    {
        // Act
        var result = new HtmlValidator().Validate(html, Default);

        // Assert
        result.Success.Should().BeFalse();
    }

    [TestCase("<body onload=\"alert(1)\"><p>Hi</p>")]
    [TestCase("<html onmouseover=\"alert(1)\"><p>Hi</p>")]
    public static void Validate_WrapperElementAttribute_Fails(string html)
    {
        /* An html or body start tag does not create an element - the parser merges its attributes onto the
           document's existing one. The same merge happens again in the browser when the stored markup is
           written into a page, so an attribute that survives here is served as an attribute of the real
           element. */

        // Act
        var result = new HtmlValidator().Validate(html, Default);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Test]
    public static void Validate_JavascriptHref_Fails()
    {
        // Act
        var result = new HtmlValidator().Validate("<a href=\"javascript:alert(1)\">Go</a>", Default);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("href");
    }

    [Test]
    public static void Validate_LinksNotAllowed_RejectsTheAnchor()
    {
        // Act
        var result = new HtmlValidator().Validate(
            "<p>Text <a href=\"https://example.com\">link</a></p>",
            new HtmlValidatorOptions { AllowLinks = false });

        // Assert
        result.Success.Should().BeFalse();
    }

    [Test]
    public static void Validate_TagWhitelist_IsAnAllowListNotAnExemptionList()
    {
        // Arrange - the behaviour the option name always implied. Previously the list only exempted tags
        // from a fixed deny-list, so naming "b" here left every unlisted tag accepted as well.
        var options = new HtmlValidatorOptions
        {
            AllowLinks = true,
            TagWhitelist = ["b"]
        };

        // Act
        var result = new HtmlValidator().Validate("<b>Bold</b><p>Para</p>", options);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("p");
    }

    [TestCase("<p>Good</p>")]
    [TestCase("<p>Line<br>break</p>")]
    [TestCase("<table><tbody><tr><td>x</td></tr></tbody></table>")]
    [TestCase("<ul><li>One<li>Two</ul>")]
    [TestCase("<p>Para", Description = "End tag omitted entirely, not partially.")]
    [TestCase("<p>{group.name}</p>")]
    public static void Validate_RequireWellFormed_AcceptsLegalMarkup(string html)
    {
        // Arrange - HTML5 gives p and li optional end tags and br no end tag at all, so none of these
        // is a mistake. A check that rejected them would be worse than no check.
        // Act
        var result = new HtmlValidator().Validate(html, WellFormed);

        // Assert
        result.Success.Should().BeTrue(result.Message);
    }

    [TestCase("<p><a href=\"https://x.com\">x</a></p", Description = "Closing tag never terminated.")]
    [TestCase("<p><b>mismatched</p></b>", Description = "Closed out of order.")]
    [TestCase("<b>Bold", Description = "No optional end tag, so leaving it open is an error.")]
    public static void Validate_RequireWellFormed_RejectsMalformedMarkup(string html)
    {
        // Act
        var result = new HtmlValidator().Validate(html, WellFormed);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Malformed HTML");
    }

    [Test]
    public static void Validate_WithoutRequireWellFormed_AcceptsMalformedMarkup()
    {
        // Arrange - off by default, so the fields holding editor output are unaffected.
        // Act
        var result = new HtmlValidator().Validate("<p>x</p", Default);

        // Assert
        result.Success.Should().BeTrue(result.Message);
    }

    [Test]
    public static void Validate_Empty_Passes()
    {
        // Act / Assert
        new HtmlValidator().Validate(string.Empty, Default).Success.Should().BeTrue();
    }
}
