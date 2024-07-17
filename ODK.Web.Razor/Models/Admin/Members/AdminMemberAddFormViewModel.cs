using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ODK.Web.Razor.Models.Admin.Members;

public class AdminMemberAddFormViewModel
{

    public required IReadOnlyCollection<SelectListItem> AdminMemberOptions { get; set; }

    [DisplayName("Member")]
    [Required]
    public Guid? MemberId { get; set; }
}
