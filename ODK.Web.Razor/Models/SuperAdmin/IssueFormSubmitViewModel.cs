using ODK.Core.Issues;

namespace ODK.Web.Razor.Models.SuperAdmin;

public class IssueFormSubmitViewModel
{
    public IssueStatusType? Status { get; set; }

    public IssueType? Type { get; set; }
}