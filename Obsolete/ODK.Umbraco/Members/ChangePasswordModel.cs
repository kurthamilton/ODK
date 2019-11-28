using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Umbraco.Members
{
    public class ChangePasswordModel
    {
        [Required]
        [DisplayName("New password")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string NewPassword { get; set; }

        [DisplayName("Confirm password")]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords must match")]
        public string PasswordConfirm { get; set; }
    }
}
