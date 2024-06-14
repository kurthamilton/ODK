using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.SuperAdmin;

public class EmailProviderFormViewModel
{
    [Display(Name = "Batch size")]
    [Range(1, int.MaxValue)]
    public int? BatchSize { get; set; }

    [Required]
    [Display(Name = "Daily limit")]
    [Range(1, int.MaxValue)]
    public int? DailyLimit { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "From email address")]
    public string FromEmailAddress { get; set; } = "";

    [Required]
    [Display(Name = "From name")]
    public string FromName { get; set; } = "";

    [Required]
    [Display(Name = "SMTP login")]
    public string SmtpLogin { get; set; } = "";

    [Required]
    [Display(Name = "SMTP password")]
    public string SmtpPassword { get; set; } = "";

    [Required]
    [Range(1, int.MaxValue)]
    [Display(Name = "SMTP port")]
    public int? SmtpPort { get; set; }

    [Required]
    [Display(Name = "SMTP server")]
    public string SmtpServer { get; set; } = "";
}
