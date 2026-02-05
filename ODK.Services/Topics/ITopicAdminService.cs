using ODK.Services.Topics.Models;
using ODK.Services.Topics.ViewModels;

namespace ODK.Services.Topics;

public interface ITopicAdminService
{
    Task<ServiceResult> AddTopic(IMemberServiceRequest request, Guid topicGroupId, string name);

    Task<ServiceResult> AddTopicGroup(IMemberServiceRequest request, string name);

    Task ApproveTopics(IMemberServiceRequest request, ApproveTopicsModel approved, ApproveTopicsModel rejected);

    Task<TopicAdminPageViewModel> GetTopicAdminPageViewModel(IMemberServiceRequest request, Guid topicId);

    Task<TopicsAdminPageViewModel> GetTopicsAdminPageViewModel(IMemberServiceRequest request);

    Task<ServiceResult> UpdateTopic(IMemberServiceRequest request, Guid topicId, TopicUpdateModel model);
}