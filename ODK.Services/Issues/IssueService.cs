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
            return ServiceResult.Failure("Title and message must be set");
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
    }
}
