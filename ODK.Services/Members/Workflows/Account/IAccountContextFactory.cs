using ODK.Core.Members;
using ODK.Services.Members.Models;

namespace ODK.Services.Members.Workflows.Account;

public interface IAccountContextFactory
{
    /// <summary>
    /// For an invited member accepting their invitation, which activates the account an import raised and
    /// joins the group in one act. The caller resolves the invitation first, for the same reason an activation
    /// link is resolved first: an invitation that names no account has no state for the machine to read.
    /// </summary>
    Task<AccountContext> CreateForAcceptInvite(
        IChapterServiceRequest request, MemberChapterInvite invite, InvitationAcceptModel model);

    /// <summary>
    /// For following an activation link inside a group. The caller resolves the token first - a link that
    /// names no account has no state for the machine to read - and this loads the rest.
    /// </summary>
    Task<AccountContext> CreateForChapterActivation(
        IChapterServiceRequest request, MemberActivationToken token, string password);

    /// <summary>For following an activation link on the site, which tells no group about anybody.</summary>
    Task<AccountContext> CreateForSiteActivation(
        IServiceRequest request, MemberActivationToken token, string password);

    AccountContext CreateForImport(IChapterServiceRequest request, MemberImportModel import, ImportBatch batch);

    Task<AccountContext> CreateForGroupSignUp(IChapterServiceRequest request, MemberCreateProfile profile);

    Task<AccountContext> CreateForSiteSignUp(IServiceRequest request, AccountCreateModel profile);
}
