using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Admin.Members;

public class AdminMemberFormSubmitViewModel
{
    [DisplayName("Admin email address")]
    [EmailAddress]
    public string? AdminEmailAddress { get; set; }

    public string? Name { get; set; }

    [DisplayName("Receive contact emails")]
    public bool ReceiveContactEmails { get; set; }

    [DisplayName("Receive event comment emails")]
    public bool ReceiveEventCommentEmails { get; set; }

    [DisplayName("Receive new member emails")]
    public bool ReceiveNewMemberEmails { get; set; }

    [DisplayName("Role")]
    [Required]
    public ChapterAdminRole? Role { get; set; }
}
