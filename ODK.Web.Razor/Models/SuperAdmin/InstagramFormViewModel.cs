using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.SuperAdmin;

public class InstagramFormViewModel
{
    [Required]
    [Display(Name = "Scraper user agent")]
    public string ScraperUserAgent { get; set; } = string.Empty;
}
