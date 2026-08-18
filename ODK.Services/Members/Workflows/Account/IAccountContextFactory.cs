using ODK.Core.Members;
using ODK.Services.Members.Models;

namespace ODK.Services.Members.Workflows.Account;

public interface IAccountContextFactory
{
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
