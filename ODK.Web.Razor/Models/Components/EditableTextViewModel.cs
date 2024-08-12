namespace ODK.Web.Razor.Models.Components;

public class EditableTextViewModel
{
    public required string Action { get; init; }    

    public required string Id { get; init; }

    public required bool IsAdmin { get; init; }

    public required string Name { get; init; }    

    public required string? Text { get; init; }

    public required string Title { get; init; }
}
