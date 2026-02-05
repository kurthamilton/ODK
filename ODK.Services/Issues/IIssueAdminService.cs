using ODK.Services.Issues.ViewModels;

namespace ODK.Services.Issues;

public interface IIssueAdminService
{
    Task<IssueAdminPageViewModel> GetIssueAdminPageViewModel(IMemberServiceRequest request, Guid issueId);

    Task<IssuesAdminPageViewModel> GetIssuesAdminPageViewModel(IMemberServiceRequest request);

    Task<ServiceResult> ReplyToIssue(IMemberServiceRequest request, Guid issueId, string message);
}