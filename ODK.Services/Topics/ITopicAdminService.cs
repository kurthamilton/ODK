using ODK.Services.Topics.Models;
using ODK.Services.Topics.ViewModels;

namespace ODK.Services.Topics;

public interface ITopicAdminService
{
    Task<ServiceResult> AddTopic(Guid currentMemberId, Guid topicGroupId, string name);

    Task ApproveTopics(Guid currentMemberId, ApproveTopicsModel approved, ApproveTopicsModel rejected);

    Task<TopicsAdminPageViewModel> GetTopicsAdminPageViewModel(Guid currentMemberId);
}
