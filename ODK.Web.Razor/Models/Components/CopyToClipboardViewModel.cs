namespace ODK.Web.Razor.Models.Components;

public class CopyToClipboardViewModel
{
    public string? IconClass { get; init; }

    public required string Text { get; init; }

    /// <summary>
    /// What the tooltip says, where the text itself is too long to be one - a paragraph in a tooltip is
    /// unreadable. Defaults to the text, which is right for a URL.
    /// </summary>
    public string? Tooltip { get; init; }
}
