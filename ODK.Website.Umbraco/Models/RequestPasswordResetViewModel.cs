using System.ComponentModel.DataAnnotations;

namespace ODK.Website.Models
{
    public class RequestPasswordResetViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}