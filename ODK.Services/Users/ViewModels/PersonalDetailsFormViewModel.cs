using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Chapters;

namespace ODK.Services.Users.ViewModels;

public class PersonalDetailsFormViewModel
{
    public Chapter? Chapter { get; set; }

    [DisplayName("Email address")]
    [EmailAddress]
    public string EmailAddress { get; set; } = string.Empty;

    [DisplayName("Receive event emails")]
    public bool EmailOptIn { get; set; } = true;

    [Required]
    [DisplayName("First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [DisplayName("Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [DisplayName("Privacy policy")]
    public bool PrivacyPolicy { get; set; }

    /// <summary>
    /// reCAPTCHA token, populated client-side by odk.recaptcha.js. Deliberately nullable and NOT [Required]:
    /// a non-nullable string is implicitly required by model validation, which would block signup whenever
    /// no token is posted - i.e. always in the e2e environment, where reCAPTCHA is disabled. A missing token
    /// is handled server-side instead (verification fails and the member is flagged, never blocked).
    /// </summary>
    public string? Recaptcha { get; set; }
}
