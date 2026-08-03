using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Members;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.Core.Repositories;

public interface IMemberSiteSubscriptionRecordRepository
    : IReadWriteRepository<MemberSiteSubscriptionRecord, IMemberSiteSubscriptionRecordQueryBuilder>
{
    IDeferredQueryMultiple<MemberSiteSubscriptionDto> GetAllChapterOwnerSubscriptionDtos(PlatformType platform);

    IDeferredQuerySingleOrDefault<MemberSiteSubscriptionDto> GetDtoByMemberId(Guid memberId);
}
