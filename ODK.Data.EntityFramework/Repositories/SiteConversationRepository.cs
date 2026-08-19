using Microsoft.EntityFrameworkCore;
using ODK.Core.Messages;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Messages;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class SiteConversationRepository :
    ReadWriteRepositoryBase<SiteConversation, ISiteConversationQueryBuilder>, ISiteConversationRepository
{
    public SiteConversationRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<SiteConversationDto> GetDtos(bool archived)
        => Query()
            .Archived(archived)
            .ToDto()
            .ByLatestActivity()
            .GetAll();

    public IDeferredQueryMultiple<SiteConversationDto> GetDtosByMemberId(Guid memberId)
        => Query()
            .ForMember(memberId)
            .ToDto()
            .ByLatestActivity()
            .GetAll();

    public override ISiteConversationQueryBuilder Query()
        => CreateQueryBuilder(context => new SiteConversationQueryBuilder(context));
}
