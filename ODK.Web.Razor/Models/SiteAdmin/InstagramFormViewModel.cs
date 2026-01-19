using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class InstagramFormViewModel
{
    [Required]
    [Display(Name = "Scraper user agent")]
    public string ScraperUserAgent { get; set; } = string.Empty;
}