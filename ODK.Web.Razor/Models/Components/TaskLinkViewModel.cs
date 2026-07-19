namespace ODK.Web.Razor.Models.Components;

public class TaskLinkViewModel
{
    public IconType? Icon { get; init; }

    public required string Path { get; init; }

    public required string Text { get; init; }
}
