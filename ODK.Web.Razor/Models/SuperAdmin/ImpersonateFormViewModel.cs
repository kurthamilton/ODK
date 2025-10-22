using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.SuperAdmin;

public class ImpersonateFormViewModel
{
    [Display(Name = "Member Id")]
    [Required]
    public Guid? MemberId { get; set; }
}
