using ODK.Core.Members;
using ODK.Core.Workflows;
using ODK.Services.Subscriptions;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Puts the account being activated on the platform's default site subscription. Reads the account off the
/// context rather than the sign-up's new one, because most edges that reach this find the account they
/// activate - only an OAuth-verified sign-up creates and activates in one transition.
/// </summary>
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
            MemberId = context.RequiredAccount.Id,
            SiteSubscriptionId = context.RequiredSiteSubscription.Id
        });

        return StepOutcome.Continue();
    }
}
