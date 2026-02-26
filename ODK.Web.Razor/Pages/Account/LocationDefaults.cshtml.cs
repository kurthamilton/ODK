namespace ODK.Web.Razor.Pages.Account;

public class LocationDefaultsModel : OdkSiteAccountPageModel
{
    public double Lat { get; private set; }

    public double Long { get; private set; }

    public void OnGet(double lat, double @long)
    {
        Lat = lat;
        Long = @long;
    }
}