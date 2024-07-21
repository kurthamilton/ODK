namespace ODK.Web.Razor.Models.Header;

public class HeaderViewModel
{
    public string? ImageLink { get; set; }

    public string? ImageUrl { get; set; }

    public required NavbarViewModel Navbar { get; set; }
}
