using ODK.Core;
using ODK.Core.Issues;
using ODK.Core.Platforms;
using ODK.Data.Core;
using ODK.Services.Issues.Models;
using ODK.Services.Members;

namespace ODK.Services.Issues;

public class IssueService : IIssueService
{
    private readonly IMemberEmailService _memberEmailService;
    private readonly IPlatformProvider _platformProvider;
    private readonly IUnitOfWork _unitOfWork;

    public IssueService(
        IUnitOfWork unitOfWork,
        IMemberEmailService memberEmailService,
        IPlatformProvider platformProvider)
    {
        _memberEmailService = memberEmailService;
        _platformProvider = platformProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> CreateIssue(Guid currentMemberId, IssueCreateModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Message))
        {
            return ServiceResult.Failure("Title and message cannot be empty");
        }

        var platform = _platformProvider.GetPlatform();

        var (member, siteEmailSettings) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.SiteEmailSettingsRepository.Get(platform));

        var issue = new Issue
        {
            CreatedUtc = DateTime.UtcNow,
            MemberId = currentMemberId,
            Status = IssueStatusType.New,
            Title = model.Title,
            Type = model.Type
        };

        _unitOfWork.IssueRepository.Add(issue);

        var issueMessage = new IssueMessage
        {
            CreatedUtc = issue.CreatedUtc,
            IssueId = issue.Id,
            MemberId = currentMemberId,
            Text = model.Message
        };

        _unitOfWork.IssueMessageRepository.Add(issueMessage);

        await _unitOfWork.SaveChangesAsync();

        await _memberEmailService.SendNewIssueEmail(member, issue, issueMessage, siteEmailSettings);

        return ServiceResult.Successful();
    }

    public async Task<IssuePageViewModel> GetIssuePageViewModel(Guid currentMemberId, Guid issueId)
    {
        var (currentMember, issue, messages) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.IssueRepository.GetById(issueId),
            x => x.IssueMessageRepository.GetByIssueId(issueId));

        OdkAssertions.MeetsCondition(issue, x => x.MemberId == currentMemberId);

        return new IssuePageViewModel
        {
            CurrentMember = currentMember,
            Issue = issue,
            Messages = messages
        };
    }

    public async Task<IssuesPageViewModel> GetIssuesPageViewModel(Guid currentMemberId)
    {
        var issues = await _unitOfWork.IssueRepository.GetByMemberId(currentMemberId).Run();

        return new IssuesPageViewModel
        {
            Issues = issues
        };
    }

    public async Task<ServiceResult> ReplyToIssue(Guid currentMemberId, Guid issueId, string message)
    {
        var platform = _platformProvider.GetPlatform();

        var (issue, siteEmailSettings) = await _unitOfWork.RunAsync(
            x => x.IssueRepository.GetById(issueId),
            x => x.SiteEmailSettingsRepository.Get(platform));

        OdkAssertions.MeetsCondition(issue, x => x.MemberId == currentMemberId);

        if (string.IsNullOrWhiteSpace(message))
        {
            return ServiceResult.Failure("Message cannot be empty");
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

        await _memberEmailService.SendIssueReply(issue, issueMessage, null, siteEmailSettings);

        return ServiceResult.Successful();
    }
}
