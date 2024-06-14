using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.SuperAdmin;

public class InstagramFormViewModel
{
    [Display(Name = "Scrape on demand")]
    public bool Scrape { get; set; }

    [Required]
    [Display(Name = "Scraper user agent")]
    public string ScraperUserAgent { get; set; } = "";
}
