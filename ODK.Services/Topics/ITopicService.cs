using ODK.Services.Topics.Models;

namespace ODK.Services.Topics;

public interface ITopicService
{
    Task AddNewChapterTopics(IMemberChapterServiceRequest request, IReadOnlyCollection<NewTopicModel> models);

    Task AddNewMemberTopics(IMemberServiceRequest request, IReadOnlyCollection<NewTopicModel> models);
}