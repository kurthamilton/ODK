using ODK.Services.Topics.ViewModels;

namespace ODK.Services.Topics;

public interface ITopicAdminService
{
    Task<ServiceResult> AddTopic(Guid currentMemberId, Guid topicGroupId, string name);

    Task<TopicsAdminPageViewModel> GetTopicsAdminPageViewModel(Guid currentMemberId);
}
