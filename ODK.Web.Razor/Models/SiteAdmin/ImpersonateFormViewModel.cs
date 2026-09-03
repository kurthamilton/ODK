using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class ImpersonateFormViewModel
{
    [Required]
    public Guid? MemberId { get; set; }
}
