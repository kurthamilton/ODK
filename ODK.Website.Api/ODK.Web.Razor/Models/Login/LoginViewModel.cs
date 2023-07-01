using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Login
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [DisplayName("Email address")]
        public string? Email { get; set; }

        [Required]
        [PasswordPropertyText]
        public string? Password { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
