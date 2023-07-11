using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Admin.Members
{
    public class SendMemberEmailFormViewModel
    {
        [Required]
        public string? Body { get; set; }

        [Required]
        public string? Subject { get; set; }
    }
}
