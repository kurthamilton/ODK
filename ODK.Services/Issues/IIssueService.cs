using ODK.Services.Issues.Models;
using ODK.Services.Issues.ViewModels;

namespace ODK.Services.Issues;

public interface IIssueService
{
    Task<ServiceResult> CreateIssue(IMemberServiceRequest request, IssueCreateModel model);

    Task<IssuePageViewModel> GetIssuePageViewModel(Guid currentMemberId, Guid issueId);

    Task<IssuesPageViewModel> GetIssuesPageViewModel(Guid currentMemberId);

    Task<ServiceResult> ReplyToIssue(IMemberServiceRequest request, Guid issueId, string message);
}