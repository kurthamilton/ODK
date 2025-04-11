﻿using ODK.Core.Members;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IMemberSubscriptionRecordRepository : IReadWriteRepository<MemberSubscriptionRecord>
{
    IDeferredQuerySingle<MemberSubscriptionRecord> GetByExternalId(string externalId);

    IDeferredQuerySingleOrDefault<MemberSubscriptionRecord> GetLatest(Guid memberId, Guid chapterId);
}
