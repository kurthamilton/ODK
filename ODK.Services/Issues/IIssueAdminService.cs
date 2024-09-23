using ODK.Services.Issues.Models;

namespace ODK.Services.Issues;

public interface IIssueAdminService
{
    Task<IssueAdminPageViewModel> GetIssueAdminPageViewModel(Guid currentMemberId, Guid issueId);

    Task<IssuesAdminPageViewModel> GetIssuesAdminPageViewModel(Guid currentMemberId);

    Task<ServiceResult> ReplyToIssue(Guid currentMemberId, Guid issueId, string message);
}
