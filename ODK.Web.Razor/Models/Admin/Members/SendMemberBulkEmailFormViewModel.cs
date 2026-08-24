namespace ODK.Web.Razor.Models.Admin.Members;

public class SendMemberBulkEmailFormViewModel : SendMemberEmailFormViewModel
{
    /// <summary>
    /// The members the checkbox column has selected. Empty is a valid post - the send is refused with a
    /// message rather than treated as "everyone".
    /// </summary>
    public List<Guid> MemberIds { get; set; } = [];
}
