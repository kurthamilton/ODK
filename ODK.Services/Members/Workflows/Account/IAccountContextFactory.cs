using ODK.Services.Members.Models;

namespace ODK.Services.Members.Workflows.Account;

public interface IAccountContextFactory
{
    AccountContext CreateForImport(IChapterServiceRequest request, MemberImportModel import, ImportBatch batch);

    Task<AccountContext> CreateForGroupSignUp(IChapterServiceRequest request, MemberCreateProfile profile);

    Task<AccountContext> CreateForSiteSignUp(IServiceRequest request, AccountCreateModel profile);
}
