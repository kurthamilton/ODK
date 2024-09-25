using ODK.Services.Issues.Models;

namespace ODK.Services.Issues;

public interface IIssueService
{
    Task<ServiceResult> CreateIssue(Guid currentMemberId, IssueCreateModel model);

    Task<IssuePageViewModel> GetIssuePageViewModel(Guid currentMemberId, Guid issueId);

    Task<IssuesPageViewModel> GetIssuesPageViewModel(Guid currentMemberId);

    Task<ServiceResult> ReplyToIssue(Guid currentMemberId, Guid issueId, string message);
}
