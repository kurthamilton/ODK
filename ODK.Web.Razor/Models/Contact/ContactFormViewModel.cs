using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Contact
{
    public class ContactFormViewModel
    {
        [Required]
        [EmailAddress]
        [DisplayName("Email address")]
        public string? EmailAddress { get; set; }

        [Required]
        public string? Message { get; set; }

        [Required]
        public string? Recaptcha { get; set; }
    }
}
