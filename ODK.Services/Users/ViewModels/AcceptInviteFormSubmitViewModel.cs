using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Services.Users.ViewModels;

/// <summary>
/// Exactly what the accept-invitation form posts: the first password on the account an import raised, and the
/// name the member confirmed.
/// </summary>
/// <remarks>
/// The email address is deliberately absent. It is the invitation's trust anchor - the token proves the link
/// reached the address the import supplied - so a form that could change it would have the token prove nothing.
/// </remarks>
public class AcceptInviteFormSubmitViewModel
{
    [DataType(DataType.Password)]
    [DisplayName("Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [DisplayName("First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [DisplayName("Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DisplayName("Privacy policy")]
    public bool PrivacyPolicy { get; set; }

    /// <summary>The token the invitation link carried, posted back so the submit spends the same invitation.</summary>
    public string Token { get; set; } = string.Empty;
}
