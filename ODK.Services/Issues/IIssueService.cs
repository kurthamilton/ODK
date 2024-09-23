using ODK.Services.Issues.Models;

namespace ODK.Services.Issues;

public interface IIssueService
{
    Task<ServiceResult> CreateIssue(Guid currentMemberId, IssueCreateModel model);
}
