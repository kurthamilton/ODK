using ODK.Core.Members;
using ODK.Core.Workflows;
using ODK.Services.Subscriptions;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>Puts the new account on the platform's default site subscription.</summary>
public sealed class MakeSiteSubscriptionCurrent : IStep<AccountContext>
{
    private readonly IMemberSiteSubscriptionWriter _memberSiteSubscriptionWriter;

    public MakeSiteSubscriptionCurrent(IMemberSiteSubscriptionWriter memberSiteSubscriptionWriter)
    {
        _memberSiteSubscriptionWriter = memberSiteSubscriptionWriter;
    }

    public static string Description => "puts the account on the default site subscription";

    public static StepKind Kind => StepKind.Write;

    public async Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        await _memberSiteSubscriptionWriter.MakeRecordCurrent(new MemberSiteSubscriptionRecord
        {
            CreatedUtc = DateTime.UtcNow,
            MemberId = context.RequiredNewMember.Id,
            SiteSubscriptionId = context.RequiredSiteSubscription.Id
        });

        return StepOutcome.Continue();
    }
}
