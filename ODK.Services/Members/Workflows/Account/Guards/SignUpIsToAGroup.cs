using ODK.Core.Workflows;

namespace ODK.Services.Members.Workflows.Account.Guards;

/// <summary>
/// Whether the sign-up is to a group rather than to the site. The two submit different forms and do almost
/// entirely different work - a group sign-up brings an avatar and the group's questions and joins the group,
/// a site sign-up brings topics, a location and a referral - so they are separate edges.
/// </summary>
public sealed class SignUpIsToAGroup : IGuard<AccountContext>
{
    public string Description => "to a group";

    public bool IsSatisfied(AccountContext context) => context.Chapter != null;
}
