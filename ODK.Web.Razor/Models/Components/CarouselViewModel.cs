using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components;

public class CarouselViewModel
{
    public IHtmlContent? Footer { get; init; }

    public required string Id { get; init; }

    public required IReadOnlyCollection<CarouselItemViewModel> Items { get; init; }
}