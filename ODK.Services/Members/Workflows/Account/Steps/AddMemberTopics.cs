using ODK.Core.Members;
using ODK.Core.Workflows;
using ODK.Data.Core;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>The interests the sign-up picked from the list already on offer.</summary>
public sealed class AddMemberTopics : IStep<AccountContext>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddMemberTopics(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public static string Description => "records the member's interests";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        if (context.Topics.Count > 0)
        {
            _unitOfWork.MemberTopicRepository.AddMany(context.Topics.Select(x => new MemberTopic
            {
                MemberId = context.RequiredNewMember.Id,
                TopicId = x.Id
            }));
        }

        return Task.FromResult(StepOutcome.Continue());
    }
}
