using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Services.Users.ViewModels;

public class ProfileFormSubmitViewModel
{
    [DisplayName("Email address")]
    public string EmailAddress { get; set; } = "";

    [DisplayName("Receive event emails")]
    public bool EmailOptIn { get; set; } = true;

    [Required]
    [DisplayName("First Name")]
    public string FirstName { get; set; } = "";

    [Required]
    [DisplayName("Last Name")]
    public string LastName { get; set; } = "";

    [Required]
    public bool PrivacyPolicy { get; set; }

    public List<ProfileFormPropertyViewModel> Properties { get; set; } = new();

    [Required]
    public bool SubscriptionPolicy { get; set; }

    [Required]
    public bool ThreeTenets { get; set; }
}
