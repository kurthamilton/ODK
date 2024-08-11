namespace ODK.Web.Razor.Pages.Groups
{
    public class IndexModel : OdkPageModel
    {
        public double? Lat {  get; private set; }

        public double? Long { get; private set; }

        public string? LocationName { get; private set; }

        public void OnGet(double? lat, double? @long, string? name)
        {
            Lat = lat;
            LocationName = name;
            Long = @long;
        }
    }
}
