using ODK.Core.Members;
using ODK.Services.Issues.Models;

namespace ODK.Services.Issues;

public interface IIssueAdminService
{
    Task<IssueAdminPageViewModel> GetIssueAdminPageViewModel(MemberServiceRequest request, Guid issueId);

    Task<IssuesAdminPageViewModel> GetIssuesAdminPageViewModel(MemberServiceRequest request);

    Task<ServiceResult> ReplyToIssue(MemberServiceRequest request, Guid issueId, string message);
}
