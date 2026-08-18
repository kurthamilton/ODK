using ODK.Core.Workflows;

namespace ODK.Services.Members.Workflows.Account.Guards;

/// <summary>
/// Whether the activation happened inside a group rather than on the site. The two do different work either
/// side of the shared password write: a group hears about its new member, while a site account is simply
/// welcomed - so they are separate edges.
/// </summary>
public sealed class ActivationIsForAGroup : IGuard<AccountContext>
{
    public string Description => "in a group";

    public bool IsSatisfied(AccountContext context) => context.Chapter != null;
}
