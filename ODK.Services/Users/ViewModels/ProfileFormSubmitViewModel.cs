using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Services.Users.ViewModels;

public class ProfileFormSubmitViewModel
{
    [DisplayName("Email address")]
    public string EmailAddress { get; set; } = "";

    [Required]
    [DisplayName("First name")]
    public string FirstName { get; set; } = "";

    [Required]
    [DisplayName("Last name")]
    public string LastName { get; set; } = "";

    [Required]
    [DisplayName("Privacy policy")]
    public bool PrivacyPolicy { get; set; }
}
