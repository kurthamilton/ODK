namespace ODK.Web.Razor.Models.Components;

public class CarouselViewModel
{
    public required string Id { get; init; }

    public required IReadOnlyCollection<CarouselItemViewModel> Items { get; init; }
}