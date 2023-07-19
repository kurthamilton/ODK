using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.SuperAdmin
{
    public class PaymentSettingsFormViewModel
    {
        public string? Provider { get; set; }

        [Required]
        [DisplayName("Public key")]
        public string? PublicKey { get; set;}

        [Required]
        [DisplayName("Secret key")]
        public string? SecretKey { get; set; }
    }
}
