namespace ODK.Web.Razor.Models.Header;

public class HeaderViewModel
{
    public string? ImageAltText { get; init; }

    public string? ImageLink { get; init; }

    public string? ImageUrl { get; init; }

    public required NavbarViewModel Navbar { get; init; }
}
