namespace ODK.Web.Razor.Models.Components;

public class HeadingViewModel
{
    public string? Class { get; init; }

    public required string Title { get; init; }

    /// <summary>
    /// Absent renders the title in a div, so a caller with no view on the level gets text rather than a
    /// heading the document outline has to account for.
    /// </summary>
    public HeadingType? Type { get; init; }
}