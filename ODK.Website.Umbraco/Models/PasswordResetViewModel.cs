using System.ComponentModel.DataAnnotations;

namespace ODK.Website.Models
{
    public class PasswordResetViewModel
    {
        [Compare(nameof(Password))]
        [Display(Name = "Confirm")]
        [Required]
        public string ConfirmPassword { get; set; }

        [MinLength(6, ErrorMessage = "Your password must be at least 6 characters long")]
        [Required]
        public string Password { get; set; }
    }
}