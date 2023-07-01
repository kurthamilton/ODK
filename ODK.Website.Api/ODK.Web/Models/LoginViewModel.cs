using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Models
{
    public class LoginViewModel
    {
        [Required]
        public string? Email { get; set; }

        [Required]
        [PasswordPropertyText]
        public string? Password { get; set; }
    }
}
