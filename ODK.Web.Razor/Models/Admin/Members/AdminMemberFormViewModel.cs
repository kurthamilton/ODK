using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Admin.Members;

public class AdminMemberFormViewModel
{
    [DisplayName("Admin email address")]
    [Required]
    [EmailAddress]
    public string AdminEmailAddress { get; set; } = "";

    public string? Name { get; set; }

    [DisplayName("Receive contact emails")]
    public bool ReceiveContactEmails { get; set; }

    [DisplayName("Receive new member emails")]
    public bool ReceiveNewMemberEmails { get; set; }

    [DisplayName("Send new member emails")]
    public bool SendNewMemberEmails { get; set; }
}
