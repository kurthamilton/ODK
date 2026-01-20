using Microsoft.AspNetCore.Html;
using ODK.Web.Common.Components;

namespace ODK.Web.Razor.Models.Components;

public class ModalViewModel
{
    public required Func<object?, IHtmlContent> Body { get; init; }

    public string? Class { get; init; }

    public Func<object?, IHtmlContent>? Footer { get; init; }

    public required string Id { get; init; }

    public bool HideFooter { get; init; }

    public ModalSize? Size { get; init; }

    public required string Title { get; init; }
}