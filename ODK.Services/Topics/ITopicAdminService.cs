using ODK.Services.Topics.Models;
using ODK.Services.Topics.ViewModels;

namespace ODK.Services.Topics;

public interface ITopicAdminService
{
    Task<ServiceResult> AddTopic(MemberServiceRequest request, Guid topicGroupId, string name);

    Task<ServiceResult> AddTopicGroup(MemberServiceRequest request, string name);

    Task ApproveTopics(MemberServiceRequest request, ApproveTopicsModel approved, ApproveTopicsModel rejected);

    Task<TopicAdminPageViewModel> GetTopicAdminPageViewModel(MemberServiceRequest request, Guid topicId);

    Task<TopicsAdminPageViewModel> GetTopicsAdminPageViewModel(MemberServiceRequest request);

    Task<ServiceResult> UpdateTopic(MemberServiceRequest request, Guid topicId, UpdateTopicModel model);
}