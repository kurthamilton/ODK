namespace ODK.Web.Razor.Models.Components;

public class CarouselItemViewModel
{
    public string? Caption { get; init; }

    public required string? ImageAlt { get; init; }

    public required string ImageUrl { get; init; }
}