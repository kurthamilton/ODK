using ODK.Services.Topics.Models;

namespace ODK.Services.Topics;

public interface ITopicService
{
    Task AddNewChapterTopics(MemberChapterServiceRequest request, IReadOnlyCollection<NewTopicModel> models);

    Task AddNewMemberTopics(MemberServiceRequest request, IReadOnlyCollection<NewTopicModel> models);
}
