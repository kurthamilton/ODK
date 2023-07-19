using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Login
{
    public class LoginViewModel
    {
        public Chapter? Chapter { get; set; }

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
