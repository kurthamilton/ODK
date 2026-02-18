namespace ODK.Web.Razor.Models.Components;

public class TabsViewModel
{
    public string? Class { get; init; }

    public required IReadOnlyCollection<MenuItem> MenuItems { get; init; }
}
