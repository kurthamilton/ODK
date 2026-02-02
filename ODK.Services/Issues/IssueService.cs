using ODK.Core;
using ODK.Core.Issues;
using ODK.Data.Core;
using ODK.Services.Issues.Models;
using ODK.Services.Issues.ViewModels;
using ODK.Services.Members;

namespace ODK.Services.Issues;

public class IssueService : IIssueService
{
    private readonly IMemberEmailService _memberEmailService;
    private readonly IUnitOfWork _unitOfWork;

    public IssueService(
        IUnitOfWork unitOfWork,
        IMemberEmailService memberEmailService)
    {
        _memberEmailService = memberEmailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> CreateIssue(MemberServiceRequest request, IssueCreateModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Message))
        {
            return ServiceResult.Failure("Title and message cannot be empty");
        }

        var (currentMember, platform) = (request.CurrentMember, request.Platform);

        var siteEmailSettings = await _unitOfWork.SiteEmailSettingsRepository.Get(platform).Run();

        var issue = new Issue
        {
            CreatedUtc = DateTime.UtcNow,
            MemberId = currentMember.Id,
            Status = IssueStatusType.New,
            Title = model.Title,
            Type = model.Type
        };

        _unitOfWork.IssueRepository.Add(issue);

        var issueMessage = new IssueMessage
        {
            CreatedUtc = issue.CreatedUtc,
            IssueId = issue.Id,
            MemberId = currentMember.Id,
            Text = model.Message
        };

        _unitOfWork.IssueMessageRepository.Add(issueMessage);

        await _unitOfWork.SaveChangesAsync();

        await _memberEmailService.SendNewIssueEmail(
            request,
            currentMember,
            issue,
            issueMessage,
            siteEmailSettings);

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

    public async Task<ServiceResult> ReplyToIssue(MemberServiceRequest request, Guid issueId, string message)
    {
        var (currentMember, platform) = (request.CurrentMember, request.Platform);

        var (issue, siteEmailSettings) = await _unitOfWork.RunAsync(
            x => x.IssueRepository.GetById(issueId),
            x => x.SiteEmailSettingsRepository.Get(platform));

        OdkAssertions.MeetsCondition(issue, x => x.MemberId == currentMember.Id);

        if (string.IsNullOrWhiteSpace(message))
        {
            return ServiceResult.Failure("Message cannot be empty");
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

        await _memberEmailService.SendIssueReply(
            request,
            issue,
            issueMessage,
            null,
            siteEmailSettings);

        return ServiceResult.Successful();
    }
}
