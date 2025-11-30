using ODK.Services.Issues.Models;

namespace ODK.Services.Issues;

public interface IIssueService
{
    Task<ServiceResult> CreateIssue(MemberServiceRequest request, IssueCreateModel model);

    Task<IssuePageViewModel> GetIssuePageViewModel(Guid currentMemberId, Guid issueId);

    Task<IssuesPageViewModel> GetIssuesPageViewModel(Guid currentMemberId);

    Task<ServiceResult> ReplyToIssue(MemberServiceRequest request, Guid issueId, string message);
}
