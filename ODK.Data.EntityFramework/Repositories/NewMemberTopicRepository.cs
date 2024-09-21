using ODK.Core.Topics;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class NewMemberTopicRepository : ReadWriteRepositoryBase<NewMemberTopic>, INewMemberTopicRepository
{
    public NewMemberTopicRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQueryMultiple<NewMemberTopic> GetAll() => Set()
        .DeferredMultiple();

    public IDeferredQueryMultiple<NewMemberTopic> GetByMemberId(Guid memberId) => Set()
        .Where(x => x.MemberId == memberId)
        .DeferredMultiple();
}
