using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components;

public class ModalViewModel
{
    public required Func<object?, IHtmlContent> Body { get; set; }

    public required string Id { get; init; }

    /// <summary>
    /// modal-ms, modal-lg, modal-xl
    /// </summary>
    public string? Size { get; set; }

    public required string Title { get; init; }
}
