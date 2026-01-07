using ODK.Core.Members;
using ODK.Data.Core.Repositories;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberPrivacySettingsRepository : WriteRepositoryBase<MemberChapterPrivacySettings>, IMemberPrivacySettingsRepository
{
    public MemberPrivacySettingsRepository(OdkContext context)
        : base(context)
    {
    }
}
