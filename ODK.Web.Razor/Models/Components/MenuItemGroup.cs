namespace ODK.Web.Razor.Models.Components;

public class MenuItemGroup
{
    public required IReadOnlyCollection<MenuItem> MenuItems { get; init; }

    public string? Title { get; init; }
}