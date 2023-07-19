using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Account
{
    public class ActivateFormViewModel
    {
        [Required]
        public string? Password { get; set; }
    }
}
