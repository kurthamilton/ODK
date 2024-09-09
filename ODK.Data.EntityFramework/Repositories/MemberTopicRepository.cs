using ODK.Core.Members;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class MemberTopicRepository : WriteRepositoryBase<MemberTopic>, IMemberTopicRepository
{
    public MemberTopicRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<MemberTopic> GetByMemberId(Guid memberId) => Set()
        .Where(x => x.MemberId == memberId)
        .DeferredMultiple();

    public int Merge(IEnumerable<MemberTopic> existing, Guid memberId, IEnumerable<Guid> topicIds)
    {
        var changes = 0;

        var existingDictionary = existing
            .ToDictionary(x => x.TopicId);

        foreach (var topicId in topicIds)
        {
            if (existingDictionary.ContainsKey(topicId))
            {
                continue;
            }

            Add(new MemberTopic
            {
                MemberId = memberId,
                TopicId = topicId
            });

            changes++;
        }

        foreach (var existingMemberTopic in existing)
        {
            if (topicIds.Contains(existingMemberTopic.TopicId))
            {
                continue;
            }

            Delete(existingMemberTopic);
            changes++;
        }

        return changes;
    }
}
