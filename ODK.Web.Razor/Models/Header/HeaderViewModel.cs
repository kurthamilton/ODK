namespace ODK.Web.Razor.Models.Header;

public class HeaderViewModel : OdkComponentViewModel
{
    public HeaderViewModel(OdkComponentContext context) 
        : base(context)
    {
    }

    public string? ImageAltText { get; init; }

    public string? ImageLink { get; init; }

    public string? ImageUrl { get; init; }

    public required NavbarViewModel Navbar { get; init; }
}
