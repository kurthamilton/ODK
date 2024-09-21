using ODK.Services.Topics.Models;

namespace ODK.Services.Topics;

public interface ITopicService
{
    Task AddNewChapterTopics(Guid currentMemberId, Guid chapterId, IReadOnlyCollection<NewTopicModel> models);

    Task AddNewMemberTopics(Guid currentMemberId, IReadOnlyCollection<NewTopicModel> models);
}
