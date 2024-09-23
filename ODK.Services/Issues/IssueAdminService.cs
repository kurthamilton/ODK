using ODK.Core.Issues;
using ODK.Core.Platforms;
using ODK.Data.Core;
using ODK.Services.Issues.Models;
using ODK.Services.Members;

namespace ODK.Services.Issues;

public class IssueAdminService : OdkAdminServiceBase, IIssueAdminService
{
    private readonly IMemberEmailService _memberEmailService;
    private readonly IPlatformProvider _platformProvider;
    private readonly IUnitOfWork _unitOfWork;

    public IssueAdminService(
        IUnitOfWork unitOfWork,
        IPlatformProvider platformProvider,
        IMemberEmailService memberEmailService)
        : base(unitOfWork)
    {
        _memberEmailService = memberEmailService;
        _platformProvider = platformProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<IssueAdminPageViewModel> GetIssueAdminPageViewModel(Guid currentMemberId, Guid issueId)
    {
        var (issue, messages) = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.IssueRepository.GetById(issueId),
            x => x.IssueMessageRepository.GetByIssueId(issueId));

        return new IssueAdminPageViewModel
        {
            Issue = issue,
            Messages = messages
        };
    }

    public async Task<IssuesAdminPageViewModel> GetIssuesAdminPageViewModel(Guid currentMemberId)
    {
        var issues = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.IssueRepository.GetAll());

        return new IssuesAdminPageViewModel
        {
            Issues = issues
        };
    }

    public async Task<ServiceResult> ReplyToIssue(Guid currentMemberId, Guid issueId, string message)
    {
        var issue = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.IssueRepository.GetById(issueId));

        if (string.IsNullOrWhiteSpace(message))
        {
            return ServiceResult.Failure("Message must be set");
        }

        var issueMessage = new IssueMessage
        {
            CreatedUtc = DateTime.UtcNow,
            IssueId = issueId,
            MemberId = currentMemberId,
            Text = message
        };

        _unitOfWork.IssueMessageRepository.Add(issueMessage);
        await _unitOfWork.SaveChangesAsync();

        var platform = _platformProvider.GetPlatform();

        var (member, siteEmailSettings) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(issue.MemberId),
            x => x.SiteEmailSettingsRepository.Get(platform));

        await _memberEmailService.SendIssueReply(issue, issueMessage, member, siteEmailSettings);

        return ServiceResult.Successful();
    }
}
