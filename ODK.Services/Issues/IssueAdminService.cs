using ODK.Core.Issues;
using ODK.Data.Core;
using ODK.Services.Issues.Models;
using ODK.Services.Members;

namespace ODK.Services.Issues;

public class IssueAdminService : OdkAdminServiceBase, IIssueAdminService
{
    private readonly IMemberEmailService _memberEmailService;
    private readonly IUnitOfWork _unitOfWork;

    public IssueAdminService(
        IUnitOfWork unitOfWork,
        IMemberEmailService memberEmailService)
        : base(unitOfWork)
    {
        _memberEmailService = memberEmailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IssueAdminPageViewModel> GetIssueAdminPageViewModel(MemberServiceRequest request, Guid issueId)
    {
        var (issue, messages) = await GetSiteAdminRestrictedContent(request,
            x => x.IssueRepository.GetById(issueId),
            x => x.IssueMessageRepository.GetByIssueId(issueId));

        var member = await _unitOfWork.MemberRepository.GetById(issue.MemberId).Run();

        return new IssueAdminPageViewModel
        {
            CurrentMember = request.CurrentMember,
            Issue = issue,
            Member = member,
            Messages = messages
        };
    }

    public async Task<IssuesAdminPageViewModel> GetIssuesAdminPageViewModel(MemberServiceRequest request)
    {
        var issues = await GetSiteAdminRestrictedContent(request,
            x => x.IssueRepository.GetAll());

        return new IssuesAdminPageViewModel
        {
            Issues = issues
        };
    }

    public async Task<ServiceResult> ReplyToIssue(MemberServiceRequest request, Guid issueId, string message)
    {
        var (currentMember, platform) = (request.CurrentMember, request.Platform);

        var issue = await GetSiteAdminRestrictedContent(request,
            x => x.IssueRepository.GetById(issueId));

        if (string.IsNullOrWhiteSpace(message))
        {
            return ServiceResult.Failure("Message must be set");
        }

        var issueMessage = new IssueMessage
        {
            CreatedUtc = DateTime.UtcNow,
            IssueId = issueId,
            MemberId = currentMember.Id,
            Text = message
        };

        _unitOfWork.IssueMessageRepository.Add(issueMessage);
        await _unitOfWork.SaveChangesAsync();

        var (member, siteEmailSettings) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(issue.MemberId),
            x => x.SiteEmailSettingsRepository.Get(platform));

        await _memberEmailService.SendIssueReply(
            request,
            issue,
            issueMessage,
            member,
            siteEmailSettings);

        return ServiceResult.Successful();
    }
}
