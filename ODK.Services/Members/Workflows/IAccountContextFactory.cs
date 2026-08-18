using ODK.Services.Members.Models;

namespace ODK.Services.Members.Workflows;

public interface IAccountContextFactory
{
    Task<AccountContext> CreateForJoin(
        IMemberChapterServiceRequest request,
        IEnumerable<MemberPropertyUpdateModel> properties);
}
