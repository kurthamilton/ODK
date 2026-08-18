using ODK.Core.Workflows;
using ODK.Services.Topics;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Creates the interests the sign-up typed in that were not already on offer, and attaches them. Runs after
/// the commit because the topic service writes and commits for itself, so it cannot be part of this
/// transaction.
/// </summary>
public sealed class AddNewMemberTopics : IStep<AccountContext>
{
    private readonly ITopicService _topicService;

    public AddNewMemberTopics(ITopicService topicService)
    {
        _topicService = topicService;
    }

    public static string Description => "creates the interests the member typed in";

    public static StepKind Kind => StepKind.ExternalEffect;

    public async Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        await _topicService.AddNewMemberTopics(
            MemberServiceRequest.Create(context.RequiredNewMember, context.Request),
            context.RequiredSiteProfile.NewTopics);

        return StepOutcome.Continue();
    }
}
