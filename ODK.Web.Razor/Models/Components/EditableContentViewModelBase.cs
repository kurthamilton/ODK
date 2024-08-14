using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components;

public abstract class EditableContentViewModelBase
{
    public required string Action { get; init; }

    public string? Class { get; init; }

    public required string Id { get; init; }

    public required bool IsAdmin { get; init; }

    public bool IsCreate { get; init; }

    public required string Name { get; init; }

    public required string Title { get; init; }
}
